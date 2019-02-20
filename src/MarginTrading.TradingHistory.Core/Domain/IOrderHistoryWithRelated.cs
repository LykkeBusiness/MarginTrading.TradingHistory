using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IOrderHistoryWithRelated : IOrderHistory
    {
        /// <summary>
        /// Info about take profit order
        /// </summary>
        RelatedOrderInfoWithPrice TakeProfit { get; }
        
        /// <summary>
        /// Info about stop loss order
        /// </summary>
        RelatedOrderInfoWithPrice StopLoss { get; }
    }
}
