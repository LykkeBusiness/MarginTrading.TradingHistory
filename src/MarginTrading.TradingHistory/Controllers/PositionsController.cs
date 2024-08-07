﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Route("api/positions-history")]
    [Obsolete("Will be removed.")]
    public class PositionsController : Controller, IPositionsHistoryApi
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IDealsRepository _dealsRepository;

        public PositionsController(
            IPositionsHistoryRepository positionsHistoryRepository,
            IDealsRepository dealsRepository)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
            _dealsRepository = dealsRepository;
        }
        
        /// <summary> 
        /// Get closed positions with optional filtering 
        /// </summary> 
        [HttpGet, Route("")] 
        [Obsolete("Will be removed.")]
        public async Task<List<PositionContract>> PositionHistory(
            [FromQuery] string accountId, [FromQuery] string instrument, [FromQuery] DateTime? eventDateFrom, [FromQuery] DateTime? eventDateTo)
        {
            var positions = (await _positionsHistoryRepository.GetAsync(accountId, instrument, eventDateFrom, eventDateTo))
                .Where(x => x.HistoryType == PositionHistoryType.Close || x.HistoryType == PositionHistoryType.PartiallyClose)
                .ToDictionary(x => x.DealId);
            var deals = (await _dealsRepository.GetAsync(accountId, instrument))
                .ToDictionary(x => x.DealId);

            return deals.Keys.Select(x => Convert(positions, deals, x)).Where(d => d != null).ToList();
        }

        /// <summary>
        /// Get closed position by Id
        /// </summary>
        /// <param name="positionId">Deal ID!</param>
        /// <returns></returns>
        [HttpGet, Route("{positionId}")]
        [Obsolete("Will be removed.")]
        public async Task<PositionContract> PositionById(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
            {
                throw new ArgumentException("Position id must be set", nameof(positionId));
            }

            var anyPositionEvent = (await _positionsHistoryRepository.GetAsync(positionId)).FirstOrDefault();

            if (anyPositionEvent == null)
                return null;
            
            var deals = (await _dealsRepository.GetAsync(anyPositionEvent.AccountId, anyPositionEvent.AssetPairId))
                .ToDictionary(x => x.DealId);

            return Convert(new Dictionary<string, IPositionHistory>
            {
                {anyPositionEvent.DealId, anyPositionEvent}
            }, deals, anyPositionEvent.DealId);
        }

        private PositionContract Convert(Dictionary<string, IPositionHistory> positions, 
            Dictionary<string, IDealWithCommissionParams> deals, string id)
        {
            if (!positions.TryGetValue(id, out var positionHistory)
                || !deals.TryGetValue(id, out var deal))
                return null;

            return new PositionContract
            {
                Id = positionHistory.DealId, //TODO: temp, think about it )
                DealId = positionHistory.DealId,
                AccountId = positionHistory.AccountId,
                Instrument = positionHistory.AssetPairId,
                Timestamp = deal.Created,
                Direction = positionHistory.Direction.ToType<PositionDirectionContract>(),
                Price = deal.ClosePrice,
                Volume = deal.Volume,
                PnL = deal.Fpl,
                FxRate = deal.CloseFxPrice,
                Margin = 0,
                TradeId = positionHistory.Id,
                RelatedOrders = positionHistory.RelatedOrders.Select(o => o.Id).ToList(),
                RelatedOrderInfos = positionHistory.RelatedOrders.Select(o =>
                    new RelatedOrderInfoContract {Id = o.Id, Type = o.Type.ToType<OrderTypeContract>()}).ToList(),
                AdditionalInfo = deal.AdditionalInfo,
                Originator = positionHistory.CloseOriginator?.ToType<OriginatorTypeContract>() ??
                             OriginatorTypeContract.Investor,
                CorrelationId = positionHistory.CorrelationId
            };
        }
    }
}
