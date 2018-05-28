using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        private const string CloseSuffix = "_close";
        
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

            return history.Where(CheckOrderUpdateType).SelectMany(MakeOrderContractsFromHistory).ToList();
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
        
        private static IEnumerable<OrderContract> MakeOrderContractsFromHistory(IOrderHistory r)
        {
            var baseOrder = Convert(r, r.Status == OrderStatus.Closed);

            if (r.StopLoss == null && r.Status == OrderStatus.Closed)
            {
                var slOrder = CreateSlTpOrder(r, true);
                baseOrder.RelatedOrders.Add(slOrder.Id);
                
                if (slOrder.Status == OrderStatusContract.Executed)
                    yield return slOrder;
            }
            if (r.TakeProfit != null && r.Status == OrderStatus.Closed)
            {
                var tpOrder = CreateSlTpOrder(r, false);
                baseOrder.RelatedOrders.Add(tpOrder.Id);
                
                if (tpOrder.Status == OrderStatusContract.Executed)
                    yield return tpOrder;
            }

            yield return baseOrder;
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
                CreatedTimestamp = history.CreateDate,
                Direction = history.Type.ToType<OrderDirectionContract>(),
                ExecutionPrice = history.OpenPrice,
                ExpectedOpenPrice = history.ExpectedOpenPrice,
                ForceOpen = true,
                ModifiedTimestamp = history.CloseDate ?? history.OpenDate ?? history.CreateDate, //history.UpdateTimestamp,
                Originator = OriginatorTypeContract.Investor,
                ParentOrderId = "",//history.ParentOrderId,
                PositionId = history.Id,
                RelatedOrders = new List<string>(),
                Status = Convert(history.Status),
                TradesIds = GetTrades(history.Id, history.Status, orderDirection),
                Type = history.ExpectedOpenPrice == null ? OrderTypeContract.Market : OrderTypeContract.Limit,
                ValidityTime = null,
                Volume = history.Volume,
            };
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
            result.ParentOrderId = result.Id;
            result.Id += isSlOrTp ? "_StopLoss" : "_TakeProfit";
            result.Direction = result.Direction == OrderDirectionContract.Buy
                ? OrderDirectionContract.Sell
                : OrderDirectionContract.Buy;
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
