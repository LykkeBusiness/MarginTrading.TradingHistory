// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class CorrelationContract
    {
        public string CorrelationId { get; set; }
        public CorrelationEntityTypeContract EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }     
    }
}
