using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface IPositionsApi
    {
        /// <summary>
        /// Get closed positions with optional filtering
        /// </summary>
        [Get("/api/positionHistory")]
        Task<List<PositionContract>> PositionHistory(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument);
    }
}
