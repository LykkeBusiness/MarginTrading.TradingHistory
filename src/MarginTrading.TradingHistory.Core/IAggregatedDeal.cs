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
    }
}
