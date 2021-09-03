// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// API for positions history
    /// </summary>
    [PublicAPI]
    [Obsolete("Will be removed.")]
    public interface IPositionsHistoryApi 
    { 
        /// <summary> 
        /// Get closed positions with optional filtering 
        /// </summary> 
        [Get("/api/positions-history")] 
        [Obsolete("Will be removed.")]
        Task<List<PositionContract>> PositionHistory( 
            [Query, CanBeNull] string accountId, 
            [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? eventDate);
        
        /// <summary>
        /// Get closed position by Id
        /// </summary>
        [Get("/api/positions-history/{positionId}")]
        [Obsolete("Will be removed.")]
        Task<PositionContract> PositionById([NotNull] string positionId);
    }
}
