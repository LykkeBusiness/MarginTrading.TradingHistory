// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Correlation : ICorrelation
    {
        [NotNull] public string Id { get; }
        
        [NotNull] public string CorrelationId { get; }
        
        public CorrelationEntityType EntityType { get; }
        
        [NotNull] public string EntityId { get; }
        
        public DateTime Timestamp { get; }

        public Correlation(
            [NotNull] string id,
            [NotNull] string correlationId,
            CorrelationEntityType entityType,
            [NotNull] string entityId,
            DateTime timestamp)
        {
            Id = id;
            CorrelationId = correlationId;
            EntityType = entityType;
            EntityId = entityId;
            Timestamp = timestamp;
        }
    }
}
