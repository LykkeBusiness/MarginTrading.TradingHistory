using System;
using System.Data;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class RelatedOrderExtendedInfo : RelatedOrderInfo
    {
        public decimal ExpectedOpenPrice { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public DateTime ModifiedTimestamp { get; set; }
    }
}