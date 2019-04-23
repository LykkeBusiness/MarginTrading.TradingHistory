using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Route("api/position-events")]
    public class PositionEventsController : Controller, IPositionEventsApi
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IConvertService _convertService;
        
        public PositionEventsController(
            IPositionsHistoryRepository positionsHistoryRepository,
            IConvertService convertService)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
            _convertService = convertService;
        }

        /// <summary> 
        /// Get all position events with optional filtering by accountId and instrument.
        /// </summary> 
        [HttpGet, Route("")] 
        public async Task<List<PositionEventContract>> PositionHistory(
            [FromQuery] string accountId, [FromQuery] string instrument)
        {
            var orders = await _positionsHistoryRepository.GetAsync(accountId, instrument);

            return orders.Select(Convert).Where(d => d != null).ToList();
        }

        /// <summary> 
        /// Get paginated position events with optional filtering by accountId and instrument.
        /// </summary> 
        [HttpGet, Route("by-pages")] 
        public async Task<PaginatedResponseContract<PositionEventContract>> PositionHistoryByPages(
            [FromQuery] string accountId, [FromQuery] string instrument,
            int? skip = null, int? take = null)
        {
            ApiValidationHelper.ValidatePagingParams(skip, take);
            
            var data = await _positionsHistoryRepository.GetByPagesAsync(accountId, instrument, skip, take);

            return new PaginatedResponseContract<PositionEventContract>(
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
                HistoryType = positionHistory.HistoryType.ToType<PositionHistoryTypeContract>()
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
