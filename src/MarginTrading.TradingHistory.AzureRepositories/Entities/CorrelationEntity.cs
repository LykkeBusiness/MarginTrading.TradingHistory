// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class CorrelationEntity : AzureTableEntity, ICorrelation
    {
        public string Id 
        {
            get => RowKey;
            set => RowKey = value;
        }
        public string CorrelationId { get; set; }
        public CorrelationEntityType EntityType { get ; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
