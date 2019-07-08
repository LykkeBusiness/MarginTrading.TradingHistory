// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class RelatedOrderExtendedInfo : RelatedOrderInfo
    {
        public decimal ExpectedOpenPrice { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public DateTime ModifiedTimestamp { get; set; }
    }
}