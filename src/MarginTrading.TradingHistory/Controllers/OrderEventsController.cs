using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    /// <summary>
    /// Expose all events associated with orders, optionally including related orders.
    /// </summary>
    [Route("api/order-events")]
    public class OrderEventsController : Controller, IOrderEventsApi
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        
        public OrderEventsController(IOrdersHistoryRepository ordersHistoryRepository)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
        }

        /// <summary>
        /// Get orders with optional filtering, optionally including related orders.
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<OrderEventContract>> OrderHistory(
            [FromQuery] string accountId = null, [FromQuery] string assetPairId = null,
            [FromQuery] bool withRelated = true)
        {
            var history = await _ordersHistoryRepository.GetHistoryAsync(accountId, assetPairId, withRelated);

            return history.Select(Convert).ToList();
        }

        /// <summary>
        /// Get order by Id, optionally including related orders.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="withRelated"></param>
        /// <returns></returns>
        [HttpGet, Route("{orderId}")]
        public async Task<List<OrderEventContract>> OrderById(string orderId, [FromQuery] bool withRelated = true)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ArgumentException("Order id must be set", nameof(orderId));
            }

            var history = await _ordersHistoryRepository.GetHistoryAsync(orderId, withRelated);

            return history.Select(Convert).ToList();
        }

        private static OrderEventContract Convert(IOrderHistory history)
        {
            if (history == null)
                return null;

            return new OrderEventContract
            {
                Id = history.Id,
                AccountId = history.AccountId,
                AssetPairId = history.AssetPairId,
                ParentOrderId = history.ParentOrderId,
                PositionId = history.Id,
                Direction = history.Direction.ToType<OrderDirectionContract>(),
                Type = history.Type.ToType<OrderTypeContract>(),
                Status = history.Status.ToType<OrderStatusContract>(),
                Originator = history.Originator.ToType<OriginatorTypeContract>(),
                CancellationOriginator = history.CancellationOriginator.ToType<OriginatorTypeContract>(),
                Volume = history.Volume,
                ExpectedOpenPrice = history.ExpectedOpenPrice,
                ExecutionPrice = history.ExecutionPrice,
                FxRate = history.FxRate, 
                ForceOpen = history.ForceOpen,
                ValidityTime = history.ValidityTime,
                CreatedTimestamp = history.CreatedTimestamp,
                ModifiedTimestamp = history.ModifiedTimestamp,
                Code = history.Code,
                ActivatedTimestamp = history.ActivatedTimestamp,
                ExecutionStartedTimestamp = history.ExecutionStartedTimestamp,
                ExecutedTimestamp = history.ExecutedTimestamp,
                CanceledTimestamp = history.CanceledTimestamp,
                Rejected = history.Rejected,
                TradingConditionId = history.TradingConditionId,
                AccountAssetId = history.AccountAssetId,
                EquivalentAsset = history.EquivalentAsset,
                EquivalentRate = history.EquivalentRate,
                RejectReason = history.RejectReason.ToType<OrderRejectReasonContract>(),
                RejectReasonText = history.RejectReasonText,
                Comment = history.Comment,
                ExternalOrderId = history.ExternalOrderId,
                ExternalProviderId = history.ExternalProviderId,
                MatchingEngineId = history.MatchingEngineId,
                LegalEntity = history.LegalEntity,
                RelatedOrderInfos = history.RelatedOrderInfos.Select(o =>
                    new RelatedOrderInfoContract {Id = o.Id, Type = o.Type.ToType<OrderTypeContract>()}).ToList(),
                UpdateType = history.UpdateType.ToType<OrderUpdateTypeContract>(),
                AdditionalInfo = history.AdditionalInfo
            };
        }
    }
}













