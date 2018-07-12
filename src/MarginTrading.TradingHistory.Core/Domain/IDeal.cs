using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IDeal
    {
        string DealId { get; }
        DateTime Created { get; }
        string AccountId { get; }
        string AssetPairId { get; }
        string OpenTradeId { get; }
        string CloseTradeId { get; }
        PositionDirection Direction { get; }
        decimal Volume { get; }
        OriginatorType Originator { get; }
        decimal OpenPrice { get; }
        decimal OpenFxPrice { get; }
        decimal ClosePrice { get; }
        decimal CloseFxPrice { get; }
        decimal Fpl { get; }
        string AdditionalInfo { get; }
    }
}
