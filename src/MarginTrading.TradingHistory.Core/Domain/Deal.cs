using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Deal : IDeal
    {
        [NotNull] public string DealId { get; }
        public DateTime Created { get; }
        [NotNull] public string AccountId { get; }
        [NotNull] public string AssetPairId { get; }
        [NotNull] public string OpenTradeId { get; }
        [NotNull] public string CloseTradeId { get; }
        public decimal Volume { get; }
        public decimal OpenPrice { get; }
        public decimal OpenFxPrice { get; }
        public decimal ClosePrice { get; }
        public decimal CloseFxPrice { get; }
        public decimal Fpl { get; }
        [NotNull] public string AdditionalInfo { get; }

        public Deal([NotNull] string dealId, DateTime created, [NotNull] string accountId,
            [NotNull] string assetPairId, [NotNull] string openTradeId,
            string closeTradeId, decimal volume, decimal openPrice, decimal openFxPrice, decimal closePrice,
            decimal closeFxPrice, decimal fpl, string additionalInfo)
        {
            DealId = dealId ?? throw new ArgumentNullException(nameof(dealId));
            Created = created;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            OpenTradeId = openTradeId ?? throw new ArgumentNullException(nameof(openTradeId));
            CloseTradeId = closeTradeId;
            Volume = volume;
            OpenPrice = openPrice;
            OpenFxPrice = openFxPrice;
            ClosePrice = closePrice;
            CloseFxPrice = closeFxPrice;
            Fpl = fpl;
            AdditionalInfo = additionalInfo;
        }
    }
}
