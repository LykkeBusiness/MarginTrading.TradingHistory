using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface IOrdersApi
    {
        /// <summary>
        /// Get executed orders with optional filtering
        /// </summary>
        [Get("/api/orderHistory")]
        Task<List<OrderContract>> OrderHistory(
            [Query, CanBeNull] string accountId = null,
            [Query, CanBeNull] string assetPairId = null);
    }
}
