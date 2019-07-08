// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IDeal
    {
        string DealId { get; }
        DateTime Created { get; }
        string AccountId { get; }
        string AssetPairId { get; }
        
        string OpenTradeId { get; }
        OrderType OpenOrderType { get; }
        decimal OpenOrderVolume { get; }
        decimal? OpenOrderExpectedPrice { get; }
        string CloseTradeId { get; }
        OrderType CloseOrderType { get; }
        decimal CloseOrderVolume { get; }
        decimal? CloseOrderExpectedPrice { get; }
        PositionDirection Direction { get; }
        decimal Volume { get; }
        OriginatorType Originator { get; }
        decimal OpenPrice { get; }
        decimal OpenFxPrice { get; }
        decimal ClosePrice { get; }
        decimal CloseFxPrice { get; }
        decimal Fpl { get; }
        string AdditionalInfo { get; }
        decimal PnlOfTheLastDay { get; }
        
        decimal? OvernightFees { get; }
        decimal? Commission { get; }
        decimal? OnBehalfFee { get; }
        decimal? Taxes { get; }
    }
}
