using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Localization.Internal;

namespace MarginTrading.TradingHistory.Controllers
{
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
        public async Task<List<TradeContract>> List([FromQuery] string accountId, [FromQuery] string assetPairId = null)
        {
            accountId.RequiredNotNullOrWhiteSpace(nameof(accountId));
            
            var history = await _tradesRepository.GetByAccountAsync(accountId, assetPairId);

            return history.Select(Convert).ToList();
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
                Volume = tradeEntity.Volume
            };
        }
    }
}
