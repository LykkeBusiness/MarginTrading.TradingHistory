namespace MarginTrading.TradingHistory.Core.Domain
{
    public class RelatedOrderInfoWithPrice : RelatedOrderInfo
    {
        public decimal ExpectedOpenPrice { get; set; }
    }
}