using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Trade : ITrade
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public string OrderId { get; set; }
        public string PositionId { get; set; }
        public string AssetPairId { get; set; }
        public TradeType Type { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }
}
