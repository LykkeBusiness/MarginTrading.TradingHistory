// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/position-events")]
    public class PositionEventsController : Controller, IPositionEventsApi
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;

        public PositionEventsController(IPositionsHistoryRepository positionsHistoryRepository)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
        }

        /// <summary>
        /// Get all position events with optional filtering by accountId, instrument and event date period.
        /// </summary>
        [HttpGet, Route("")] 
        public async Task<List<PositionEventContract>> PositionHistory([FromQuery] string accountId, [FromQuery] string instrument, [FromQuery] DateTime? eventDateFrom, [FromQuery] DateTime? eventDateTo)
        {
            var orders = await _positionsHistoryRepository.GetAsync(accountId, instrument, eventDateFrom, eventDateTo);

            return orders.Select(Convert).Where(d => d != null).ToList();
        }

        /// <summary> 
        /// Get paginated position events with optional filtering by accountId, instrument and event date period.
        /// </summary> 
        [HttpGet, Route("by-pages")] 
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<PositionEventContract>> PositionHistoryByPages([FromQuery] string accountId, [FromQuery] string instrument, [FromQuery] DateTime? eventDateFrom, [FromQuery] DateTime? eventDateTo, int? skip = null, int? take = null)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);
            
            var data = await _positionsHistoryRepository.GetByPagesAsync(accountId, instrument, eventDateFrom, eventDateTo, skip, take);

            return new Lykke.Contracts.Responses.PaginatedResponse<PositionEventContract>(
                contents: data.Contents.Where(d => d != null).Select(Convert).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        /// <summary>
        /// Get position events by PositionId.
        /// </summary>
        /// <param name="positionId"></param>
        /// <returns></returns>
        [HttpGet, Route("{positionId}")]
        public async Task<List<PositionEventContract>> PositionById(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
            {
                throw new ArgumentException("Position id must be set", nameof(positionId));
            }

            var position = await _positionsHistoryRepository.GetAsync(positionId);

            return position.Select(Convert).ToList();
        }

        private PositionEventContract Convert(IPositionHistory positionHistory)
        {
            if (positionHistory == null)
                return null;

            return new PositionEventContract
            {
                Id = positionHistory.Id,
                Code = positionHistory.Code,
                AssetPairId = positionHistory.AssetPairId,
                Direction = positionHistory.Direction.ToType<PositionDirectionContract>(),
                Volume = positionHistory.Volume,
                AccountId = positionHistory.AccountId,
                TradingConditionId = positionHistory.TradingConditionId,
                AccountAssetId = positionHistory.AccountAssetId,
                ExpectedOpenPrice = positionHistory.ExpectedOpenPrice,
                OpenMatchingEngineId = positionHistory.OpenMatchingEngineId,
                OpenDate = positionHistory.OpenDate,
                OpenTradeId = positionHistory.OpenTradeId,
                OpenPrice = positionHistory.OpenPrice,
                OpenFxPrice = positionHistory.OpenFxPrice,
                EquivalentAsset = positionHistory.EquivalentAsset,
                OpenPriceEquivalent = positionHistory.OpenPriceEquivalent,
                RelatedOrders = positionHistory.RelatedOrders.Select(Convert).ToList(),
                LegalEntity = positionHistory.LegalEntity,
                OpenOriginator = positionHistory.OpenOriginator.ToType<OriginatorTypeContract>(),
                ExternalProviderId = positionHistory.ExternalProviderId,
                SwapCommissionRate = positionHistory.SwapCommissionRate,
                OpenCommissionRate = positionHistory.OpenCommissionRate,
                CloseCommissionRate = positionHistory.CloseCommissionRate,
                CommissionLot = positionHistory.CommissionLot,
                CloseMatchingEngineId = positionHistory.CloseMatchingEngineId,
                ClosePrice = positionHistory.ClosePrice,
                CloseFxPrice = positionHistory.CloseFxPrice,
                ClosePriceEquivalent = positionHistory.ClosePriceEquivalent,
                StartClosingDate = positionHistory.StartClosingDate,
                CloseDate = positionHistory.CloseDate,
                CloseOriginator = positionHistory.CloseOriginator?.ToType<OriginatorTypeContract>(),
                CloseReason = positionHistory.CloseReason.ToType<PositionCloseReasonContract>(),
                CloseComment = positionHistory.CloseComment,
                CloseTrades = positionHistory.CloseTrades,
                LastModified = positionHistory.LastModified,
                TotalPnL = positionHistory.TotalPnL,
                ChargedPnl = positionHistory.ChargedPnl,
                HistoryType = positionHistory.HistoryType.ToType<PositionHistoryTypeContract>(),
                DealId = positionHistory.DealId,
                Timestamp = positionHistory.HistoryTimestamp,
                CorrelationId = positionHistory.CorrelationId
            };
        }

        private RelatedOrderInfoContract Convert(RelatedOrderInfo relatedOrderInfo)
        {
            return new RelatedOrderInfoContract
            {
                Id = relatedOrderInfo.Id,
                Type = relatedOrderInfo.Type.ToType<OrderTypeContract>()
            };
        }
    }
}
