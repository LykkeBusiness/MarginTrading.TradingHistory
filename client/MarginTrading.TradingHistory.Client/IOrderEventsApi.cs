using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Common;
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
        /// Get orders with optional filtering and with pagination.
        /// </summary>
        [Post("/api/order-events")]
        Task<PaginatedResponseContract<OrderEventWithAdditionalContract>> OrderHistoryByPages(
            [Body] [CanBeNull] OrderEventsFilterRequest filters,
            [Query] [CanBeNull] int? skip = 0,
            [Query] [CanBeNull] int? take = 20,
            [Query] bool isAscending = false);

        /// <summary>
        /// Get order by Id, optionally including related orders.
        /// </summary>
        [Get("/api/order-events/{orderId}")]
        Task<List<OrderEventWithAdditionalContract>> OrderById([NotNull] string orderId,
            [Query] [CanBeNull] OrderStatusContract? status = null);
    }
}
