// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ApiLibrary.Validation;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/trades/")]
    public class TradesController : Controller, ITradesApi
    {
        private readonly ITradesRepository _tradesRepository;
        private readonly IConvertService _convertService;

        public TradesController(
            ITradesRepository tradesRepository,
            IConvertService convertService)
        {
            _tradesRepository = tradesRepository;
            _convertService = convertService;
        }
        
        /// <summary>
        /// Get a trade by <param name="tradeId"/>  
        /// </summary> 
        [HttpGet, Route("{tradeId}")] 
        public async Task<TradeContract> Get(string tradeId)
        {
            if (string.IsNullOrWhiteSpace(tradeId))
            {
                throw new ArgumentException("Trade id must be set", nameof(tradeId));
            }

            var trade = await _tradesRepository.GetAsync(tradeId);

            return Convert(trade);
        } 
        
        /// <summary>
        /// Get trades by <param name="accountId"/> with optional filtering by <param name="assetPairId"/> 
        /// </summary>
        [HttpGet, Route("")]
        [ValidateModel]
        public async Task<List<TradeContract>> List([FromQuery] [Required] string accountId, [FromQuery] string assetPairId = null)
        {
            var history = await _tradesRepository.GetByAccountAsync(accountId, assetPairId);

            return history.Select(Convert).ToList();
        }

        /// <summary>
        /// Get trades by <param name="accountId"/> with optional filtering by <param name="assetPairId"/> and pagination
        /// </summary>
        [HttpGet, Route("by-pages")]
        [ValidateModel]
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<TradeContract>> ListByPages([FromQuery] [Required] string accountId, 
            [FromQuery] string assetPairId = null, [FromQuery] int? skip = null, [FromQuery] int? take = null,
            [FromQuery] bool isAscending = false)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            var data = await _tradesRepository.GetByPagesAsync(accountId, assetPairId, skip, take,
                isAscending);

            return new Lykke.Contracts.Responses.PaginatedResponse<TradeContract>(
                contents: data.Contents.Select(Convert).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        private TradeContract Convert(ITrade tradeEntity)
        {
            return new TradeContract
            {
                Id = tradeEntity.Id,
                AccountId = tradeEntity.AccountId,
                OrderId = tradeEntity.Id,
                PositionId = tradeEntity.Id,
                AssetPairId = tradeEntity.AssetPairId,
                Type = tradeEntity.Type.ToType<TradeTypeContract>(),
                Timestamp = tradeEntity.TradeTimestamp,
                Price = tradeEntity.Price,
                Volume = tradeEntity.Volume,
                AdditionalInfo = tradeEntity.AdditionalInfo,
                CancelledBy = tradeEntity.CancelledBy,
                ExternalOrderId = tradeEntity.ExternalOrderId,
                CorrelationId = tradeEntity.CorrelationId
            };
        }
    }
}
