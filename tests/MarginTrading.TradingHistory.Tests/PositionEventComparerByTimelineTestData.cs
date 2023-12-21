using System.Collections.Generic;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;

namespace MarginTrading.TradingHistory.Tests
{
    public static class PositionEventComparerByTimelineTestData
    {
        public static IEnumerable<object[]> AllHistoryTypeComparisons()
        {
            yield return new object[] { PositionHistoryTypeContract.Open, PositionHistoryTypeContract.Close, PositionEventComparerByTimeline.FirstLessThanSecond };
            yield return new object[] { PositionHistoryTypeContract.Open, PositionHistoryTypeContract.PartiallyClose, PositionEventComparerByTimeline.FirstLessThanSecond };
            yield return new object[] { PositionHistoryTypeContract.PartiallyClose, PositionHistoryTypeContract.Close, PositionEventComparerByTimeline.FirstLessThanSecond };
            yield return new object[] { PositionHistoryTypeContract.Close, PositionHistoryTypeContract.Open, PositionEventComparerByTimeline.FirstGreaterThanSecond };
            yield return new object[] { PositionHistoryTypeContract.Close, PositionHistoryTypeContract.PartiallyClose, PositionEventComparerByTimeline.FirstGreaterThanSecond };
            yield return new object[] { PositionHistoryTypeContract.PartiallyClose, PositionHistoryTypeContract.Open, PositionEventComparerByTimeline.FirstGreaterThanSecond };
            yield return new object[] { PositionHistoryTypeContract.Close, PositionHistoryTypeContract.Close, PositionEventComparerByTimeline.FirstEqualToSecond };
            yield return new object[] { PositionHistoryTypeContract.PartiallyClose, PositionHistoryTypeContract.PartiallyClose, PositionEventComparerByTimeline.FirstEqualToSecond };
            yield return new object[] { PositionHistoryTypeContract.Open, PositionHistoryTypeContract.Open, PositionEventComparerByTimeline.FirstEqualToSecond };
        }
    }
}