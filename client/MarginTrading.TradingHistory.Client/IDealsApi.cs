﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// API for the deals
    /// </summary>
    [PublicAPI]
    public interface IDealsApi
    {
        /// <summary> 
        /// Get deals with optional filtering 
        /// </summary> 
        [Get("/api/deals")] 
        Task<List<DealContract>> List( 
            [Query, CanBeNull] string accountId, [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? closeTimeStart = null, [Query, CanBeNull] DateTime? closeTimeEnd = null);
        
        /// <summary> 
        /// Get deals total PnL with optional filtering 
        /// </summary> 
        [Get("/api/deals/totalPnl")] 
        Task<decimal> GetTotalPnL( 
            [Query, CanBeNull] string accountId, [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? closeTimeStart = null, [Query, CanBeNull] DateTime? closeTimeEnd = null);
        
        /// <summary> 
        /// Get deals with optional filtering and pagination 
        /// </summary> 
        [Get("/api/deals/by-pages")] 
        Task<PaginatedResponseContract<DealContract>> ListByPages( 
            [Query, CanBeNull] string accountId, [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? closeTimeStart = null, [Query, CanBeNull] DateTime? closeTimeEnd = null,
            [Query, CanBeNull] int? skip = null, [Query, CanBeNull] int? take = null,
            [Query] bool isAscending = false);

        /// <summary> 
        /// Get aggregated deals by account and asset pair id with optional filtering and pagination 
        /// </summary> 
        [Get("/api/deals/aggregated")]
        Task<PaginatedResponseContract<AggregatedDealContract>> GetAggregated(
            [Query, NotNull] string accountId, [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? closeTimeStart = null, [Query, CanBeNull] DateTime? closeTimeEnd = null,
            [Query, CanBeNull] int? skip = null, [Query, CanBeNull] int? take = null,
            [Query] bool isAscending = false);

        /// <summary>
        /// Get deal by Id
        /// </summary>
        [Get("/api/deals/{dealId}")]
        Task<DealContract> ById([NotNull] string dealId);
    }
}
