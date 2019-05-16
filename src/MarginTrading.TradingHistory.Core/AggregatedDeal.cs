using System;
using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Core
{
    public class AggregatedDeal : IAggregatedDeal
    {
        public string AccountId { get; }
        public string AssetPairId { get; }
        public decimal Volume { get; }
        public decimal Fpl { get; }
        public decimal FplTc { get; }
        public decimal PnlOfTheLastDay { get; }
        public decimal? OvernightFees { get; }
        public decimal? Commission { get; }
        public decimal? OnBehalfFee { get; }
        public decimal? Taxes { get; }

        public AggregatedDeal([NotNull] string accountId, [NotNull] string assetPairId,
            decimal volume, decimal fpl, decimal fplTc, decimal pnlOfTheLastDay,
            decimal? overnightFees = null, decimal? commission = null, decimal? onBehalfFee = null, decimal? taxes = null)
        {
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            Volume = volume;
            Fpl = fpl;
            FplTc = fplTc;
            PnlOfTheLastDay = pnlOfTheLastDay;
            OvernightFees = overnightFees;
            Commission = commission;
            OnBehalfFee = onBehalfFee;
            Taxes = taxes;
        }
    }
}
