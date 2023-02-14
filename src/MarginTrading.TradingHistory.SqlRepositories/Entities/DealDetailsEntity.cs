// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class DealDetailsEntity : IDealDetails
    {
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public string PositionId { get; set; }
        public string DealId { get; set; }
        public int DealSize { get; set; }
        public DateTime DealTimestamp { get; set; }
        public string OpenTradeId { get; set; }
        public DateTime OpenTimestamp { get; set; }
        OrderDirection IDealDetails.OpenDirection => (OrderDirection) Enum.Parse(typeof(OrderDirection), OpenDirection);
        public string OpenDirection { get; set; }
        public int OpenSize { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenContractVolume { get; set; }
        OrderDirection IDealDetails.TotalOpenDirection => (OrderDirection) Enum.Parse(typeof(OrderDirection), TotalOpenDirection);
        public string TotalOpenDirection { get; set; }
        public int TotalOpenSize { get; set; }
        public decimal TotalOpenPrice { get; set; }
        public decimal TotalOpenContractVolume { get; set; }
        public string CloseTradeId { get; set; }
        public DateTime CloseTimestamp { get; set; }
        OrderDirection IDealDetails.CloseDirection => (OrderDirection) Enum.Parse(typeof(OrderDirection), CloseDirection);
        public string CloseDirection { get; set; }
        public int CloseSize { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseContractVolume { get; set; }
        public decimal GrossPnLTc { get; set; }
        public decimal GrossPnLFxPrice { get; set; }
        public decimal GrossPnLSc { get; set; }
        public decimal OverallOnBehalfFees { get; set; }
        public decimal? OverallTaxes { get; set; }
        public decimal OverallFinancingCost { get; set; }
        public decimal OverallCommissions { get; set; }
        public decimal RealizedPnLDaySc { get; set; }
        public decimal RealisedPnLBtxSc { get; set; }
        public decimal NettingOfPreviouslySettledPnLs { get; set; }
        public string TaxInfo { get; set; }
    }
}
