using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderBookContract
    {
        public string Instrument { get; set; }
        public List<LimitOrderContract> Buy { get; set; }
        public List<LimitOrderContract> Sell { get; set; }
    }
}
