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
    public interface IPositionsHistoryApi 
    { 
        /// <summary> 
        /// Get positions with optional filtering 
        /// </summary> 
        [Get("/api/positions-history")] 
        Task<List<PositionContract>> PositionHistory( 
            [Query, CanBeNull] string accountId, 
            [Query, CanBeNull] string instrument);
        
        /// <summary>
        /// Get position by Id
        /// </summary>
        [Get("/api/positions-history/{positionId}")]
        Task<PositionContract> PositionById([NotNull] string positionId);
    }
}
