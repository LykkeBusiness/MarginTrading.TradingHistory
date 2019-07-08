// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        /// Get all position events with optional filtering by accountId and instrument.
        /// </summary> 
        [Get("/api/position-events")]
        Task<List<PositionEventContract>> PositionHistory(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument);

        /// <summary> 
        /// Get paginated position events with optional filtering by accountId and instrument.
        /// </summary> 
        [Get("/api/position-events/by-pages")]
        Task<PaginatedResponseContract<PositionEventContract>> PositionHistoryByPages(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument,
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
