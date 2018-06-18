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
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly IConvertService _convertService;

        public TradesController(
            IOrdersHistoryRepository ordersHistoryRepository,
            IConvertService convertService)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
            _convertService = convertService;
        }
        
        /// <summary>
        /// Get a trade by id  
        /// </summary> 
        [HttpGet, Route("{tradeId}")] 
        public async Task<TradeContract> Get(string tradeId)
        {
            if (string.IsNullOrWhiteSpace(tradeId))
            {
                throw new ArgumentException("Trade id must be set", nameof(tradeId));
            }

            var history = await _ordersHistoryRepository
                .GetHistoryAsync(x => x.UpdateType == OrderUpdateType.Executed && x.Id == tradeId);

            return history.Select(Convert).FirstOrDefault();
        } 
        
        /// <summary>
        /// Get trades with optional filtering by order or position 
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<TradeContract>> List([FromQuery] string orderId, [FromQuery] string positionId)
        {
            var history = await _ordersHistoryRepository
                .GetHistoryAsync(x => x.UpdateType == OrderUpdateType.Executed);

            return history.Select(Convert).ToList();
        }

        private TradeContract Convert(IOrderHistory tradeEntity)
        {
            return new TradeContract
            {
                Id = tradeEntity.Id,
                AccountId = tradeEntity.AccountId,
                OrderId = tradeEntity.Id,
                PositionId = tradeEntity.Id,
                AssetPairId = tradeEntity.AssetPairId,
                Type = tradeEntity.Type.ToType<TradeTypeContract>(),
                Timestamp = tradeEntity.ModifiedTimestamp,
                Price = tradeEntity.ExecutionPrice.Value,
                Volume = tradeEntity.Volume
            };
        }
    }
}
