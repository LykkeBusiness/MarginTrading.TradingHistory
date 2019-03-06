using System;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class RelatedOrderExtendedInfoContract : RelatedOrderInfoContract
    {
        public decimal Price { get; set; }
        
        public OrderStatusContract Status { get; set; }
        
        public DateTime ModifiedTimestamp { get; set; }
    }
}