// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IOrderHistoryWithAdditional : IOrderHistory
    {
        /// <summary>
        /// Info about take profit order
        /// </summary>
        RelatedOrderExtendedInfo TakeProfit { get; }
        
        /// <summary>
        /// Info about stop loss order
        /// </summary>
        RelatedOrderExtendedInfo StopLoss { get; }
        
        /// <summary>
        /// Info about execution order book spread
        /// </summary>
        decimal Spread { get; }
        
        /// <summary>
        /// Info about execution commissions
        /// </summary>
        decimal Commission { get; }
        
        /// <summary>
        /// Info about OnBehalf commissions
        /// </summary>
        decimal OnBehalf { get; }
    }
}
