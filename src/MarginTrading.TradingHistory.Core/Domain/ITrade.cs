// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface ITrade
    {
        string Id { get; }
        string AccountId { get; }
        string OrderId { get; }
        string AssetPairId { get; }
        DateTime OrderCreatedDate { get; }
        OrderType OrderType { get; }
        TradeType Type { get; }
        OriginatorType Originator { get; }
        DateTime TradeTimestamp { get; }
        decimal Price { get; }
        decimal Volume { get; }
        decimal? OrderExpectedPrice { get; }
        decimal FxRate { get; }
        string AdditionalInfo { get; }
        
        /// <summary>
        /// Order Id, that cancelled current trade
        /// </summary>
        string CancelledBy { get; }
        string ExternalOrderId { get; }
        string CorrelationId { get; }
    }
}
