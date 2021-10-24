// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        /// Order which triggered the trade
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Position id
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// Trade execution timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Instrument id (e.g."BTCUSD", where BTC - base asset unit, USD - quoting unit)
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// Trade direction from investors perspective (Buy or Sell)
        /// </summary>
        public TradeTypeContract Type { get; set; }

        /// <summary>
        /// Trade execution VWAP price (in quoting asset units per one base unit)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Order volume in base asset units
        /// </summary>
        public decimal Volume { get; set; }
        
        /// <summary>
        /// Trade additional info
        /// </summary>
        public string AdditionalInfo { get; set; }
        
        /// <summary>
        /// Order Id, that cancelled current trade
        /// </summary>
        public string CancelledBy { get; set; }

        /// <summary>
        /// External Order Id
        /// </summary>
        public string ExternalOrderId { get; set; }
        
        public string CorrelationId { get; set; }
    }
}
