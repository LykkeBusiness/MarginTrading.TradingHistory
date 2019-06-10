﻿using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Client.Models
{
    /// <summary>
    /// Info about aggregate deals
    /// </summary>
    [PublicAPI]
    public class AggregatedDealContract
    {
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Instrument id
        /// </summary>
        public string AssetPairId { get; set; }
        
        /// <summary>
        /// Order volume in base asset units 
        /// </summary>
        public decimal Volume { get; set; }
        
        /// <summary>
        /// Deal fpl
        /// </summary>
        public decimal Fpl { get; set; }

        /// <summary>
        /// Deal fpl in trading currency
        /// </summary>
        public decimal FplTc { get; set; }

        /// <summary>
        /// PnL of the day, when position was closed
        /// </summary>
        public decimal PnlOfTheLastDay { get; set; }
        
        /// <summary>
        /// Overnight swap commissions
        /// </summary>
        public decimal? OvernightFees { get; set; }
        
        /// <summary>
        /// Order execution commissions
        /// </summary>
        public decimal? Commission { get; set; }
        
        /// <summary>
        /// On behalf commissions
        /// </summary>
        public decimal? OnBehalfFee { get; set; }
        
        /// <summary>
        /// Taxes
        /// </summary>
        public decimal? Taxes { get; set; }
    }
}
