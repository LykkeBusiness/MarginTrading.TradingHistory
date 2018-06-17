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
                ? await _ordersHistoryRepository.GetHistoryAsync(accountId)
                : await _ordersHistoryRepository.GetHistoryAsync();

            if (!string.IsNullOrWhiteSpace(assetPairId))
                history = history.Where(o => o.AssetPairId == assetPairId);

            return history.Where(x => x.UpdateType == OrderUpdateType.Executed).Select(Convert).ToList();
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

            var history = await _ordersHistoryRepository
                .GetHistoryAsync(x => x.UpdateType == OrderUpdateType.Executed && x.Id == orderId);

            return history.Select(Convert).FirstOrDefault();
        }

        private static OrderContract Convert(IOrderHistory history)
        {
            return new OrderContract
            {
                Id = history.Id,
                AccountId = history.AccountId,
                AssetPairId = history.AssetPairId,
                CreatedTimestamp = history.CreatedTimestamp,
                Direction = history.Direction.ToType<OrderDirectionContract>(),
                ExecutionPrice = history.ExecutionPrice,
                ExpectedOpenPrice = history.ExpectedOpenPrice,
                FxRate = history.FxRate,
                ForceOpen = history.ForceOpen,
                ModifiedTimestamp = history.ModifiedTimestamp,
                Originator = history.Originator.ToType<OriginatorTypeContract>(),
                ParentOrderId = history.ParentOrderId,
                PositionId = history.Id,
                RelatedOrders = history.RelatedOrderInfos.Select(i => i.Id).ToList(),
                Status = history.Status.ToType<OrderStatusContract>(),
                TradesId = history.Id,
                Type = history.Type.ToType<OrderTypeContract>(),
                ValidityTime = history.ValidityTime,
                Volume = history.Volume
            };
        }
    }
}
