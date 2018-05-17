using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface ITrade
    {
        string Id { get; }
        string ClientId { get; }
        string AccountId { get; }
        string OrderId { get; }
        string PositionId { get; }
        string AssetPairId { get; }
        TradeType Type { get; }
        DateTime TradeTimestamp { get; }
        decimal Price { get; }
        decimal Volume { get; }
    }
}
