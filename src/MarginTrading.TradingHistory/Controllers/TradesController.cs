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
        /// Get a trade by id  
        /// </summary> 
        [HttpGet, Route("{tradeId}")] 
        public async Task<TradeContract> Get(string tradeId) 
        { 
            return Convert(await _tradesRepository.GetAsync(tradeId)); 
        } 
        
        /// <summary>
        /// Get trades with optional filtering by order or position 
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<TradeContract>> List([FromQuery] string orderId, [FromQuery] string positionId)
        {
            //TODO WTF is this ???
            if (orderId == null && positionId == null)
                throw new ArgumentException($"{nameof(orderId)} or {nameof(positionId)} should be passed");

            if (orderId != null && positionId != null && orderId != positionId)
                throw new ArgumentException(
                    $"{nameof(orderId)} and {nameof(positionId)} should be equal if both passed, separation is not yet supported");

            var entity = await _tradesRepository.GetAsync(orderId ?? positionId);
            return entity == null ? new List<TradeContract>() : new List<TradeContract>() {Convert(entity)};
        }
        
        private TradeContract Convert(ITrade tradeEntity)
        {
            try
            {
                return new TradeContract
                {
                    // todo: separate order from position and trade and use there ids correctly
                    Id = tradeEntity.Id,
                    ClientId = tradeEntity.ClientId,
                    AccountId = tradeEntity.AccountId,
                    OrderId = tradeEntity.Id,
                    PositionId = tradeEntity.Id,
                    AssetPairId = tradeEntity.AssetPairId,
                    Type = tradeEntity.Type.ToType<TradeTypeContract>(),
                    Timestamp = tradeEntity.TradeTimestamp,
                    Price = tradeEntity.Price,
                    Volume = tradeEntity.Volume,
                };
            }
            catch (Exception e)
            {
                return new TradeContract();
            }
        }
    }
}
