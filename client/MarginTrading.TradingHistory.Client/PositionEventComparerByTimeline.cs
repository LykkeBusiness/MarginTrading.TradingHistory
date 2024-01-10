// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MarginTrading.TradingHistory.Client.Models;

namespace MarginTrading.TradingHistory.Client
{
    /// <summary>
    /// Comparer for position events that sorts them in chronological order
    /// taking care of possible timestamp collisions.
    /// </summary>
    public class PositionEventComparerByTimeline : IComparer<PositionEventContract>
    {
        public const short FirstLessThanSecond = -1;
        public const short FirstGreaterThanSecond = 1;
        public const short FirstEqualToSecond = 0;
        
        public int Compare(PositionEventContract x, PositionEventContract y)
        {
            if (ReferenceEquals(x, y)) return FirstEqualToSecond;
            if (ReferenceEquals(null, y)) return FirstGreaterThanSecond;
            if (ReferenceEquals(null, x)) return FirstLessThanSecond;

            var samePositionId = x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase);
            return samePositionId
                ? CompareEventsOfSamePosition(x, y)
                : x.Timestamp.CompareTo(y.Timestamp);
        }
        
        private static int CompareEventsOfSamePosition(PositionEventContract x, PositionEventContract y)
        {
            var historyTypeComparison = x.HistoryType.CompareTo(y.HistoryType);
            var sameHistoryType = historyTypeComparison == FirstEqualToSecond;

            return sameHistoryType
                ? x.Timestamp.CompareTo(y.Timestamp)
                : historyTypeComparison;
        }
    }
}
