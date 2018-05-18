using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class PositionContract
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public DateTime Timestamp { get; set; }
 
        public PositionDirection Direction { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public decimal PnL { get; set; }
 
        public string TradeId { get; set; }
        public List<string> RelatedOrders { get; set; }
    }
}
