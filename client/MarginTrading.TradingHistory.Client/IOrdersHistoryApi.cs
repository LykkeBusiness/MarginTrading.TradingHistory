using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// Getting of orders history
    /// </summary>
    [PublicAPI]
    public interface IOrdersHistoryApi
    {
        /// <summary>
        /// Get executed orders with optional filtering
        /// </summary>
        [Get("/api/orders-history")]
        Task<List<OrderContract>> OrderHistory(
            [Query, CanBeNull] string accountId = null,
            [Query, CanBeNull] string assetPairId = null);
    }
}
