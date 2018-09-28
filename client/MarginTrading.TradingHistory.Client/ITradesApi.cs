using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// API for the trades
    /// </summary>
    [PublicAPI]
    public interface ITradesApi
    {
        /// <summary>
        /// Get a trade by <param name="tradeId"/> 
        /// </summary>
        [Get("/api/trades/{tradeId}")]
        Task<TradeContract> Get([NotNull] string tradeId);
        
        /// <summary>
        /// Get trades by <param name="accountId"/> with optional filtering by <param name="assetPairId"/> 
        /// </summary> 
        [Get("/api/trades/")]
        Task<List<TradeContract>> List([Query, NotNull] string accountId, [Query, CanBeNull] string assetPairId = null); 
        
        /// <summary>
        /// Get trades by <param name="accountId"/> with optional filtering by <param name="assetPairId"/> and pagination
        /// </summary> 
        [Get("/api/trades/by-pages")]
        Task<PaginatedResponseContract<TradeContract>> ListByPages([Query, NotNull] string accountId, 
            [Query, CanBeNull] string assetPairId = null,
            [Query, CanBeNull] int? skip = null, [Query, CanBeNull] int? take = null); 
    }
}
