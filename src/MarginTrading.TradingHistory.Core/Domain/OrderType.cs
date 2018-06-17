﻿namespace MarginTrading.TradingHistory.Core.Domain
{
    /// <summary>
    /// The type of order
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Market order, a basic one
        /// </summary>
        Market = 1,

        /// <summary>
        /// Limit order, a basic one
        /// </summary>
        Limit = 2,

        /// <summary>
        /// Stop order, a basic one (closing only)
        /// </summary>
        Stop = 3, 
        
        /// <summary>
        /// Take profit order, related to other order or position
        /// </summary>
        TakeProfit = 4,

        /// <summary>
        /// Stop loss order, related to other order or position
        /// </summary>
        StopLoss = 5,

        /// <summary>
        /// Trailing stop order, related to other order or position
        /// </summary>
        TrailingStop = 6,
    }
}