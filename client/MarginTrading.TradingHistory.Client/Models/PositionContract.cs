using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class PositionContract
    {
        /// <summary>
        /// Position id
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Asset pair id
        /// </summary>
        public string Instrument { get; set; }
        
        /// <summary>
        /// When the position was changed
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The direction of the position
        /// </summary>
        public PositionDirectionContract Direction { get; set; }
        
        /// <summary>
        /// Open price
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Current position volume in quoting asset units
        /// </summary>
        public decimal Volume { get; set; }
        
        /// <summary>
        /// Profit and loss of the position in base asset units (without commissions)
        /// </summary>
        public decimal PnL { get; set; }
        
        /// <summary>
        /// Current margin value
        /// </summary>
        public decimal Margin { get; set; }
        
        /// <summary>
        /// Current FxRate
        /// </summary>
        public decimal FxRate { get; set; }

        /// <summary>
        /// The trade which opened the position
        /// </summary>
        public string TradeId { get; set; }
        
        /// <summary>
        /// The related orders (sl, tp orders) 
        /// </summary>
        public List<string> RelatedOrders { get; set; }
    }
}
