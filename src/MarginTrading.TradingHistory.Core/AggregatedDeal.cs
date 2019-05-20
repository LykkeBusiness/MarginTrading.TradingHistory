namespace MarginTrading.TradingHistory.Core
{
    public class AggregatedDeal : IAggregatedDeal
    {
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public decimal Volume { get; set; }
        public decimal Fpl { get; set; }
        public decimal FplTc { get; set; }
        public decimal PnlOfTheLastDay { get; set; }
        public decimal? OvernightFees { get; set; }
        public decimal? Commission { get; set; }
        public decimal? OnBehalfFee { get; set; }
        public decimal? Taxes { get; set; }
    }
}
