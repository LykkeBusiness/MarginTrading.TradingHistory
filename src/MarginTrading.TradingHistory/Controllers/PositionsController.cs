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
    [Route("api/positions-history")]
    public class PositionsController : Controller, IPositionsHistoryApi
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IConvertService _convertService;
        
        public PositionsController(
            IPositionsHistoryRepository positionsHistoryRepository,
            IConvertService convertService)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
            _convertService = convertService;
        }
        
        /// <summary> 
        /// Get closed positions with optional filtering 
        /// </summary> 
        [HttpGet, Route("")] 
        public async Task<List<PositionContract>> PositionHistory(
            [FromQuery] string accountId, [FromQuery] string instrument)
        {
            var orders = await _positionsHistoryRepository.GetAsync(accountId, instrument);
            
            return orders.Select(Convert).ToList();
        }

        /// <summary>
        /// Get closed position by Id
        /// </summary>
        /// <param name="positionId"></param>
        /// <returns></returns>
        [HttpGet, Route("{positionId}")]
        public async Task<PositionContract> PositionById(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
            {
                throw new ArgumentException("Position id must be set", nameof(positionId));
            }

            var position = await _positionsHistoryRepository.GetAsync(positionId);

            return Convert(position);
        }

        private PositionContract Convert(IPositionHistory positionHistory)
        {
            if (positionHistory == null)
                return null;

            return new PositionContract
            {
                Id = positionHistory.Id,
                DealId = positionHistory.DealId,
                AccountId = positionHistory.AccountId,
                Instrument = positionHistory.AssetPairId,
                Timestamp = positionHistory.CloseDate ?? positionHistory.OpenDate,
                Direction = positionHistory.Direction.ToType<PositionDirectionContract>(),
                Price = positionHistory.ClosePrice == default ? positionHistory.OpenPrice : positionHistory.ClosePrice,
                Volume = positionHistory.DealInfo?.Volume ?? positionHistory.Volume,
                PnL = positionHistory.DealInfo?.Fpl ?? positionHistory.TotalPnL,
                FxRate = positionHistory.CloseFxPrice,
                Margin = 0,
                TradeId = positionHistory.Id,
                RelatedOrders = positionHistory.RelatedOrders.Select(o => o.Id).ToList(),
                RelatedOrderInfos = positionHistory.RelatedOrders.Select(o =>
                    new RelatedOrderInfoContract {Id = o.Id, Type = o.Type.ToType<OrderTypeContract>()}).ToList(),
                AdditionalInfo = positionHistory.DealInfo?.AdditionalInfo
            };
        }
    }
}
