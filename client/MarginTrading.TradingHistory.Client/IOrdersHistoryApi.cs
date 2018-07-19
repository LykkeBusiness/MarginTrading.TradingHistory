using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// API for executed orders history
    /// </summary>
    [PublicAPI]
    [Obsolete("Will be removed.")]
    public interface IOrdersHistoryApi
    {
        /// <summary>
        /// Get executed orders with optional filtering
        /// </summary>
        [Get("/api/orders-history")]
        [Obsolete("Will be removed.")]
        Task<List<OrderContract>> OrderHistory(
            [Query, CanBeNull] string accountId = null,
            [Query, CanBeNull] string assetPairId = null);

        /// <summary>
        /// Get executed order by Id
        /// </summary>
        [Get("/api/orders-history/{orderId}")]
        [Obsolete("Will be removed.")]
        Task<OrderContract> OrderById([NotNull] string orderId);
    }
}
