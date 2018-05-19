using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Client.Models
{
    /// <summary>
    /// Info about a trade
    /// </summary>
    [PublicAPI]
    public class TradeContract
    {
        /// <summary>
        /// Trade id
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Order id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Position id
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// Trade timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        //todo add other fields: volume and price
        
        public string ClientId { get; set; }
        public string AssetPairId { get; set; }
        public TradeTypeContract Type { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }
}
