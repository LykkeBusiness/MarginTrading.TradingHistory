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
        /// Deal id
        /// </summary>
        public string DealId { get; set; }
        
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Instrument id (e.g."BTCUSD", where BTC - base asset unit, USD - quoting unit)
        /// </summary>
        public string Instrument { get; set; }
        
        /// <summary>
        /// When the position was changed
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The direction of the position (Long or Short)
        /// </summary>
        public PositionDirectionContract Direction { get; set; }
        
        /// <summary>
        /// Open price (in quoting asset units per one base unit)
        /// Close price (in quoting asset units per one base unit)
        /// </summary>
        public decimal Price { get; set; }  // TODO: SPLIT TO 'OpenPrice' & 'ClosePrice'

        /// <summary>
        /// Current position volume in base asset units
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Profit and loss of the position in account asset units (without commissions)
        /// </summary>
        public decimal PnL { get; set; }

        /// <summary>
        /// Current margin value in account asset units
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
        [Obsolete]
        public List<string> RelatedOrders { get; set; }
        
        /// <summary>
        /// Related orders
        /// </summary>
        public List<RelatedOrderInfoContract> RelatedOrderInfos { get; set; }
        
        /// <summary>
        /// Close trade additional info
        /// </summary>
        public string AdditionalInfo { get; set; }
        
        /// <summary>
        /// Who initiated close of position
        /// </summary>
        public OriginatorTypeContract? Originator { get; set; }
    }
}
