// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Core
{
    public interface IAggregatedDeal
    {
        string AccountId { get; }
        string AssetPairId { get; }
        decimal Volume { get; }
        decimal Fpl { get; }
        decimal FplTc { get; }
        decimal PnlOfTheLastDay { get; }
        decimal? OvernightFees { get; }
        decimal? Commission { get; }
        decimal? OnBehalfFee { get; }
        decimal? Taxes { get; }
        int DealsCount { get; }
    }
}
