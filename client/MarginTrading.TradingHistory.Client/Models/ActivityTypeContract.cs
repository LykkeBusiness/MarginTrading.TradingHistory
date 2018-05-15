using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.TradingHistory.Client.Models
{
    public enum ActivityTypeContract
    {
        Unknown = 0,
        OrderAcceptance = 1,
        OrderActivation = 2,
        OrderCancellation = 3,
        OrderExecution = 4,
        OrderExpiry = 5,
        OrderModification = 6,
        OrderRejection = 7,
        PositionOpening = 8,
        PositionClosing = 9
    }
}
