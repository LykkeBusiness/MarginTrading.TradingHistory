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

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/trade/")]
    public class TradesController : Controller, ITradesReadingApi
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
        /// Get trades with optional filtering by order or position 
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<TradeContract>> List([FromQuery] string orderId, [FromQuery] string positionId)
        {
            if (orderId == null && positionId == null)
                throw new ArgumentException($"{nameof(orderId)} or {nameof(positionId)} should be passed");

            if (orderId != null && positionId != null && orderId != positionId)
                throw new ArgumentException(
                    $"{nameof(orderId)} and {nameof(positionId)} should be equal if both passed, separation is not yet supported");

            var id = orderId ?? positionId;
            return new List<TradeContract> {Convert(await _tradesRepository.GetAsync(id))};
        }
        
        private TradeContract Convert(ITrade tradeEntity)
        {
            return new TradeContract
            {
                // todo: separate order from position and trade and use there ids correctly
                Id = tradeEntity.Id,
                AccountId = tradeEntity.AccountId,
                OrderId = tradeEntity.Id,
                PositionId = tradeEntity.Id,
                Timestamp = tradeEntity.TradeTimestamp,
            };
        }
    }
}
