using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface ITradesApi
    {
        /// <summary>
        /// Get a trade by id  
        /// </summary>
        [Get("/api/trades/{tradeId}")]
        Task<TradeContract> Get([NotNull] string tradeId);
        
        /// <summary>
        /// Get trades with optional filtering by order or position 
        /// </summary> 
        [Get("/api/trades/")]
        Task<List<TradeContract>> List([Query, CanBeNull] string orderId, [Query, CanBeNull] string positionId); 
    }
}
