// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IOrderHistoryForOrderBlotter
    {
        string Id { get; }
        string AccountId { get; }
        string CreatedBy { get; }
        string AssetPairId { get; }
        int Volume { get; }
        OrderType Type { get; }
        OrderStatus Status { get; }
        OrderDirection Direction { get; }
        OriginatorType Originator { get; }
        decimal? ExecutionPrice { get; }
        decimal FxRate { get; }
        DateTime CreatedTimestamp { get; }
        DateTime ModifiedTimestamp { get; }
        DateTime? ValidityTime { get; }
        string Comment { get; }
        bool ForceOpen { get; }
        decimal? ExpectedOpenPrice { get; }
        string ExternalOrderId { get; }
    }
}
