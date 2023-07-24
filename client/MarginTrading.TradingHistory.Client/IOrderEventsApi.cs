// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Contracts.Responses;
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
        Task<PaginatedResponse<OrderEventWithAdditionalContract>> OrderHistoryByPages(
            [Body] [CanBeNull] OrderEventsFilterRequest filters,
            [Query] [CanBeNull] int? skip = 0,
            [Query] [CanBeNull] int? take = 20,
            [Query] bool isAscending = false);
        
        [Post("/api/order-events/for-support")]
        Task<PaginatedResponse<OrderEventForSupportContract>> OrderHistoryForSupport(
            [Body] OrderEventsForSupportRequest request);

        /// <summary>
        /// Get order by Id, optionally including related orders.
        /// </summary>
        [Get("/api/order-events/{orderId}")]
        Task<List<OrderEventWithAdditionalContract>> OrderById([NotNull] string orderId,
            [Query] [CanBeNull] OrderStatusContract? status = null);
    }
}
