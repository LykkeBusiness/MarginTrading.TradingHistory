using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class MatchedOrder
    {
        public string OrderId { get; set; }
        public string MarketMakerId { get; set; }
        public decimal LimitOrderLeftToMatch { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public string ClientId { get; set; }
        public DateTime MatchedDate { get; set; }
        public bool IsExternal { get; set; }
    }
}
