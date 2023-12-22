// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Contracts.Responses;
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
        Task<PaginatedResponse<TradeContract>> ListByPages([Query, NotNull] string accountId,
            [Query, CanBeNull] string assetPairId = null,
            [Query, CanBeNull] int? skip = null, [Query, CanBeNull] int? take = null,
            [Query] bool isAscending = false); 
        
        /// <summary>
        /// Get most traded products <param name="max"/> 
        /// </summary>
        [Get("/api/trades/most-traded-products")]
        Task<List<string>> GetMostTradedProducts([Query] DateTime date, [Query, CanBeNull] int? max = null);
    }
}
