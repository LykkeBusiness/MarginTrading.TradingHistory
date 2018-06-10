using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.Controllers
{
    /// <summary>
    /// Provides order history
    /// </summary>
    [Route("api/orders-history")]
    public class OrdersController : Controller, IOrdersHistoryApi
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly IConvertService _convertService;
        
        internal const string CloseSuffix = "_close";
        
        public OrdersController(
            IOrdersHistoryRepository ordersHistoryRepository,
            IConvertService convertService)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
            _convertService = convertService;
        }

        /// <summary>
        /// Get executed orders with optional filtering
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<OrderContract>> OrderHistory(
            [FromQuery] string accountId = null, [FromQuery] string assetPairId = null)
        {
            var history = !string.IsNullOrWhiteSpace(accountId)
                ? await _ordersHistoryRepository.GetHistoryAsync(new[] {accountId}, null, null)
                : await _ordersHistoryRepository.GetHistoryAsync();

            if (!string.IsNullOrWhiteSpace(assetPairId))
                history = history.Where(o => o.Instrument == assetPairId);

            return history.Where(CheckOrderUpdateType).Select(MakeOrderContractFromHistory).ToList();
        }

        /// <summary>
        /// Get executed order by Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet, Route("{orderId}")]
        public async Task<OrderContract> OrderById(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ArgumentException("Order id must be set", nameof(orderId));
            }

            var clearId = orderId.Replace(CloseSuffix, "");

            var history = await _ordersHistoryRepository
                .GetHistoryAsync(x => CheckOrderUpdateType(x) && x.Id == clearId);

            return history.Select(x => Convert(x, x.Status == OrderStatus.Closed)).FirstOrDefault();
        }

        private bool CheckOrderUpdateType(IOrderHistory orderHistory)
        {
            return new []
            {
                OrderUpdateType.Activate,
                OrderUpdateType.Close,
            }.Contains(orderHistory.OrderUpdateType);
        }
        
        private static OrderDirection GetOrderDirection(OrderDirection openDirection, bool isCloseOrder)
        {
            return !isCloseOrder ? openDirection :
                openDirection == OrderDirection.Buy ? OrderDirection.Sell : OrderDirection.Buy;
        }

        private static OrderContract MakeOrderContractFromHistory(IOrderHistory r)
        {
            var baseOrder = Convert(r, r.Status == OrderStatus.Closed);

            if (r.StopLoss != null && r.Status == OrderStatus.Closed)
            {
                var slOrder = CreateSlTpOrder(r, true);

                if (slOrder.Status == OrderStatusContract.Executed)
                    return slOrder;
            }

            if (r.TakeProfit != null && r.Status == OrderStatus.Closed)
            {
                var tpOrder = CreateSlTpOrder(r, false);

                if (tpOrder.Status == OrderStatusContract.Executed)
                    return tpOrder;
            }

            return baseOrder;
        }

        private static List<string> GetTrades(string orderId, OrderStatus status, OrderDirection orderDirection)
        {
            if (status == OrderStatus.WaitingForExecution)
                return new List<string>();

            return new List<string> {orderId + '_' + orderDirection};
        }
        
        private static OrderContract Convert(IOrderHistory history, bool isCloseOrder)
        {
            var orderDirection = GetOrderDirection(history.Type, isCloseOrder);
            return new OrderContract
            {
                Id = history.Id + (isCloseOrder ? CloseSuffix : ""),
                AccountId = history.AccountId,
                AssetPairId = history.Instrument,
                CreatedTimestamp = isCloseOrder ? history.CloseDate.Value : history.CreateDate,
                Direction = Convert(history.Type, isCloseOrder),
                ExecutionPrice = isCloseOrder ? history.ClosePrice : history.OpenPrice,
                ExpectedOpenPrice = isCloseOrder ? null : history.ExpectedOpenPrice,
                FxRate = history.QuoteRate > 0 ? history.QuoteRate : 1,
                ForceOpen = true,
                ModifiedTimestamp =
                    history.CloseDate ?? history.OpenDate ?? history.CreateDate, //history.UpdateTimestamp,
                Originator =
                    history.CloseReason == OrderCloseReason.CanceledByBroker ||
                    history.CloseReason == OrderCloseReason.CanceledBySystem ||
                    history.CloseReason == OrderCloseReason.ClosedByBroker ||
                    history.CloseReason == OrderCloseReason.StopOut
                        ? OriginatorTypeContract.System
                        : OriginatorTypeContract.Investor,
                ParentOrderId = "", //history.ParentOrderId,
                PositionId = history.Id,
                RelatedOrders = new List<string>(),
                Status = Convert(history.Status),
                TradesIds = GetTrades(history.Id, history.Status, orderDirection),
                Type = GetOrderType(history, isCloseOrder),
                ValidityTime = null,
                Volume = isCloseOrder ? -history.Volume : history.Volume,
            };
        }

        private static OrderTypeContract GetOrderType(IOrderHistory history, bool isCloseOrder)
        {
            if (isCloseOrder && history.CloseReason == OrderCloseReason.StopLoss)
                return OrderTypeContract.StopLoss;
            
            if (isCloseOrder && history.CloseReason == OrderCloseReason.TakeProfit)
                return OrderTypeContract.TakeProfit;
            
            return history.ExpectedOpenPrice == null || isCloseOrder
                ? OrderTypeContract.Market
                : OrderTypeContract.Limit;
        }

        private static OrderStatusContract Convert(OrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatus.WaitingForExecution:
                    return OrderStatusContract.Active;
                case OrderStatus.Active:
                    return OrderStatusContract.Executed; //TODO will be removed when orders are separated in repo
                case OrderStatus.Closed:
                    return OrderStatusContract.Executed;
                case OrderStatus.Rejected:
                    return OrderStatusContract.Rejected;
                case OrderStatus.Closing:
                    return OrderStatusContract.Active;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null);
            }
        }

        private static OrderDirectionContract Convert(OrderDirection direction, bool isCloseOrder)
        {
            if (!isCloseOrder)
            {
                return direction.ToType<OrderDirectionContract>();
            }

            if (direction == OrderDirection.Buy)
                return OrderDirectionContract.Sell;

            if (direction == OrderDirection.Sell)
                return OrderDirectionContract.Buy;

            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        private static OrderContract CreateSlTpOrder(IOrderHistory history, bool isSlOrTp)
        {
            var result = Convert(history, history.Status == OrderStatus.Closed);

            OrderStatusContract GetStatus(bool isAnySlTp)
            {
                return isAnySlTp
                    ? OrderStatusContract.Executed
                    : OrderStatusContract.Canceled;
            }

            var isAnyOfSlTp = isSlOrTp
                ? history.CloseReason == OrderCloseReason.StopLoss
                : history.CloseReason == OrderCloseReason.TakeProfit;
            result.Status = GetStatus(isAnyOfSlTp);
            result.Type = isSlOrTp ? OrderTypeContract.StopLoss : OrderTypeContract.TakeProfit;
            result.ParentOrderId = result.PositionId;
            result.Id = result.PositionId + (isSlOrTp ? "_StopLoss" : "_TakeProfit");
            result.TradesIds = new List<string>();
            result.ExpectedOpenPrice = isSlOrTp ? history.StopLoss : history.TakeProfit;
            if (isAnyOfSlTp)
            {
                result.ExecutionPrice = history.CloseReason == OrderCloseReason.StopLoss 
                    ? history.StopLoss
                    : history.TakeProfit;
            }

            return result;
        }
    }
}
