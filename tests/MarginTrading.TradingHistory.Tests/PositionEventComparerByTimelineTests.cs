// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using AutoFixture.Xunit2;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Xunit;

namespace MarginTrading.TradingHistory.Tests
{
    public class PositionEventComparerByTimelineTests
    {
        private readonly PositionEventComparerByTimeline _comparer = new PositionEventComparerByTimeline();

        [Fact]
        public void Compare_WhenBothEventsAreNull_TheyAreEqual()
        {
            var result = _comparer.Compare(null, null);
            
            Assert.Equal(PositionEventComparerByTimeline.FirstEqualToSecond, result);
        }

        [Fact]
        public void Compare_WhenFirstEventIsNull_ItIsLess()
        {
            var eventContract = new PositionEventContract();
            var result = _comparer.Compare(null, eventContract);
            
            Assert.Equal(PositionEventComparerByTimeline.FirstLessThanSecond, result);
        }

        [Fact]
        public void Compare_WhenSecondEventIsNull_NotNullEventIsGreater()
        {
            var eventContract = new PositionEventContract();
            var result = _comparer.Compare(eventContract, null);
            
            Assert.Equal(PositionEventComparerByTimeline.FirstGreaterThanSecond, result);
        }

        [Theory]
        [MemberData(nameof(PositionEventComparerByTimelineTestData.AllHistoryTypeComparisons),
            MemberType = typeof(PositionEventComparerByTimelineTestData))]
        public void Compare_WhenEventsHaveSameId_HistoryTypeCompared(
            PositionHistoryTypeContract firstEventType,
            PositionHistoryTypeContract secondEventType,
            int expectedResult)
        {
            var firstEvent = new PositionEventContract { Id = "1", HistoryType = firstEventType };
            var secondEvent = new PositionEventContract { Id = "1", HistoryType = secondEventType };
            var result = _comparer.Compare(firstEvent, secondEvent);

            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoData]
        public void Compare_WhenEventsHaveSameIdAndSameHistoryType_ReturnsComparisonOfTimestamp(
            string positionId,
            PositionHistoryTypeContract historyType,
            DateTime firstEventTimestamp,
            DateTime secondEventTimestamp)
        {
            var firstEvent = new PositionEventContract
            {
                Id = positionId, HistoryType = historyType, Timestamp = firstEventTimestamp
            };
            var secondEvent = new PositionEventContract
            {
                Id = positionId, HistoryType = historyType, Timestamp = secondEventTimestamp
            };
            var actual = _comparer.Compare(firstEvent, secondEvent);
            var expected = firstEventTimestamp.CompareTo(secondEventTimestamp);
            
            Assert.Equal(expected, actual);
        }
        
        [Theory, AutoData]
        public void Compare_WhenEventsHaveDifferentId_ReturnsComparisonOfTimestamp(
            PositionEventContract firstEvent,
            PositionEventContract secondEvent)
        {
            var actual = _comparer.Compare(firstEvent, secondEvent);
            var expected = firstEvent.Timestamp.CompareTo(secondEvent.Timestamp);
            
            Assert.Equal(expected, actual);
        }
    }
}
