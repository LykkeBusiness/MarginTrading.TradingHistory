using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Client.Models
{
    /// <summary>
    /// Info about completed deal
    /// </summary>
    [PublicAPI]
    public class DealContract
    {
        /// <summary>
        /// Deal Id
        /// </summary>
        public string DealId { get; set; }
        
        /// <summary>
        /// Deal execution timestamp
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Instrument id
        /// </summary>
        public string AssetPairId { get; set; }
        
        /// <summary>
        /// Id of the opening trade
        /// </summary>
        public string OpenTradeId { get; set; }
        
        /// <summary>
        /// Id of the closing trade
        /// </summary>
        public string CloseTradeId { get; set; }
        
        /// <summary>
        /// Order volume in base asset units 
        /// </summary>
        public decimal Volume { get; set; }
        
        /// <summary>
        /// Opening price in base asset units 
        /// </summary>
        public decimal OpenPrice { get; set; }
        
        /// <summary>
        /// Fx opening price in base asset units 
        /// </summary>
        public decimal OpenFxPrice { get; set; }
        
        /// <summary>
        /// Closing price in base asset units 
        /// </summary>
        public decimal ClosePrice { get; set; }
        
        /// <summary>
        /// Fx closing price in base asset units 
        /// </summary>
        public decimal CloseFxPrice { get; set; }
        
        /// <summary>
        /// Deal fpl
        /// </summary>
        public decimal Fpl { get; set; }
        
        /// <summary>
        /// Deal additional info
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}
