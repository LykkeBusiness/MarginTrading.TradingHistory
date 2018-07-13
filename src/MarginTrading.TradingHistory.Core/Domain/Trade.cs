using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Trade : ITrade
    {
        [NotNull] public string Id { get; }
        [NotNull] public string AccountId { get; }
        [NotNull] public string OrderId { get; }
        [NotNull] public string AssetPairId { get; }
        public DateTime OrderCreatedDate { get; }
        public OrderType OrderType { get; }
        public TradeType Type { get; }
        public OriginatorType Originator { get; }
        public DateTime TradeTimestamp { get; }
        public decimal Price { get; }
        public decimal Volume { get; }
        public decimal? OrderExpectedPrice { get; }
        public decimal FxRate { get; }
        public string AdditionalInfo { get; }

        public Trade([NotNull] string id, [NotNull] string accountId, [NotNull] string orderId,
            [NotNull] string assetPairId, DateTime orderCreatedDate, OrderType orderType,
            TradeType type, OriginatorType originator, DateTime tradeTimestamp, decimal price, decimal volume,
            decimal? orderExpectedPrice, decimal fxRate, string additionalInfo)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            OrderCreatedDate = orderCreatedDate;
            OrderType = orderType;
            Type = type;
            Originator = originator;
            TradeTimestamp = tradeTimestamp;
            Price = price;
            Volume = volume;
            OrderExpectedPrice = orderExpectedPrice;
            FxRate = fxRate;
            AdditionalInfo = additionalInfo;
        }
    }
}
