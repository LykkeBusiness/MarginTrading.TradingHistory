using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// Getting of positions history
    /// </summary>
    [PublicAPI] 
    public interface IPositionsHistoryApi 
    { 
        /// <summary> 
        /// Get closed positions with optional filtering 
        /// </summary> 
        [Get("/api/positions-history")] 
        Task<List<PositionContract>> PositionHistory( 
            [Query, CanBeNull] string accountId, 
            [Query, CanBeNull] string instrument); 
        //TODO create from closed orders 
    }
}
