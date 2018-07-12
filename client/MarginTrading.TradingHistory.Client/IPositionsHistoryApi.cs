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
            [Query, CanBeNull] string instrument);
        
        /// <summary>
        /// Get closed position by Id
        /// </summary>
        [Get("/api/positions-history/{positionId}")]
        [Obsolete("Will be removed.")]
        Task<PositionContract> PositionById([NotNull] string positionId);
    }
}
