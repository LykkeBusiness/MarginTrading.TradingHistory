// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
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
    /// API for the position events.
    /// </summary>
    [PublicAPI]
    public interface IPositionEventsApi
    {
        /// <summary> 
        /// Get all position events with optional filtering.
        /// </summary> 
        [Get("/api/position-events")]
        Task<List<PositionEventContract>> PositionHistory(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? eventDateFrom,
            [Query, CanBeNull] DateTime? eventDateTo);

        /// <summary> 
        /// Get paginated position events with optional filtering.
        /// </summary> 
        [Get("/api/position-events/by-pages")]
        Task<PaginatedResponse<PositionEventContract>> PositionHistoryByPages(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument,
            [Query, CanBeNull] DateTime? eventDateFrom,
            [Query, CanBeNull] DateTime? eventDateTo,
            [Query, CanBeNull] int? skip = null,
            [Query, CanBeNull] int? take = null);

        /// <summary>
        /// Get position events by PositionId.
        /// </summary>
        [Get("/api/position-events/{positionId}")]
        Task<List<PositionEventContract>> PositionById(
            [NotNull]string positionId);
    }
}
