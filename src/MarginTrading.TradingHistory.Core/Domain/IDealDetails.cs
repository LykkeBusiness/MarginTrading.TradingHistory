// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IDealDetails
    {
        string AccountId { get;}

        string AssetPairId { get; }

        string PositionId { get; }

        string DealId { get; }

        int DealSize { get; }

        DateTime DealTimestamp { get; }

        string OpenTradeId { get; }

        DateTime OpenTimestamp { get; }

        OrderDirection OpenDirection { get; }

        int OpenSize { get; }

        decimal OpenPrice { get; }

        decimal OpenContractVolume { get; }

        OrderDirection TotalOpenDirection { get; }

        int TotalOpenSize { get; }

        decimal TotalOpenPrice { get; }

        decimal TotalOpenContractVolume { get; }

        string CloseTradeId { get; }

        DateTime CloseTimestamp { get; }

        OrderDirection CloseDirection { get; }

        int CloseSize { get; }

        decimal ClosePrice { get; }

        decimal CloseContractVolume { get; }

        decimal GrossPnLTc { get; }

        decimal GrossPnLFxPrice { get; }

        decimal GrossPnLSc { get; }

        decimal OverallOnBehalfFees { get; }

        decimal? OverallTaxes { get; }

        decimal OverallFinancingCost { get; }

        decimal OverallCommissions { get; }

        decimal RealizedPnLDaySc { get; }

        decimal RealisedPnLBtxSc { get; }

        decimal NettingOfPreviouslySettledPnLs { get; }

        string TaxInfo { get; }
    }
}
