﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class CorrelationEntity: ICorrelation
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public CorrelationEntityType EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        
        public static CorrelationEntity Create(ICorrelation correlation)
        {
            return new CorrelationEntity
            {
                Id = correlation.Id,
                CorrelationId = correlation.CorrelationId,
                EntityType = correlation.EntityType,
                EntityId = correlation.EntityId,
                Timestamp = correlation.Timestamp
            };
        }
    }
}