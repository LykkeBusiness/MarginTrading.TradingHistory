using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// Expose all events associated with orders, optionally including related orders.
    /// </summary>
    [PublicAPI]
    public interface IOrderEventsApi
    {
        /// <summary>
        /// Get orders with optional filtering, optionally including related orders.
        /// </summary>
        [Get("/api/order-events")]
        Task<List<OrderEventContract>> OrderHistory(
            [Query, CanBeNull] string accountId = null,
            [Query, CanBeNull] string assetPairId = null,
            [Query] bool withRelated = true);

        /// <summary>
        /// Get order by Id, optionally including related orders.
        /// </summary>
        [Get("/api/order-events/{orderId}")]
        Task<List<OrderEventContract>> OrderById([NotNull] string orderId,
            [Query] bool withRelated = true);
    }
}
