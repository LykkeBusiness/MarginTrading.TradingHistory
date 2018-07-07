using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Trade : ITrade
    {
        [NotNull] public string Id { get; }
        [NotNull] public string AccountId { get; }
        [NotNull] public string OrderId { get; }
        [NotNull] public string PositionId { get; }
        [NotNull] public string AssetPairId { get; }
        public TradeType Type { get; }
        public DateTime TradeTimestamp { get; }
        public decimal Price { get; }
        public decimal Volume { get; }

        public Trade([NotNull] string id, [NotNull] string accountId,
            [NotNull] string orderId, [NotNull] string positionId, [NotNull] string assetPairId, TradeType type,
            DateTime tradeTimestamp, decimal price, decimal volume)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            PositionId = positionId;
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            Type = type;
            TradeTimestamp = tradeTimestamp;
            Price = price;
            Volume = volume;
        }
    }
}
