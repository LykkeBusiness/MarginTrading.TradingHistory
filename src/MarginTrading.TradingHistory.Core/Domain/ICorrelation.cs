// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface ICorrelation
    {
        string Id { get; }
        string CorrelationId { get; }
        CorrelationEntityType EntityType { get; }
        string EntityId { get; }
        DateTime Timestamp { get; }
    }
}
