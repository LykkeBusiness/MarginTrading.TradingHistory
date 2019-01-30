using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderEventsFilterRequest
    {
        public string AccountId { get; set; }
        
        public string AssetPairId {get; set; }

        public List<OrderStatusContract> Statuses { get; set; }

        public bool WithRelated { get; set; } = true;
        
        public DateTime? CreatedTimeStart { get; set; }
        
        public DateTime? CreatedTimeEnd { get; set; }
        
        public DateTime? ModifiedTimeStart { get; set; }
        
        public DateTime? ModifiedTimeEnd { get; set; }
        
        public List<OrderTypeContract> OrderTypes { get; set; }
        
        public List<OriginatorTypeContract> OriginatorTypes { get; set; }
    }
}