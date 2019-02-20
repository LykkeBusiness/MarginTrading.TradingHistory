namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderEventWithRelatedContract : OrderEventContract
    {
        public RelatedOrderInfoWithPriceContract TakeProfit { get; set; }
        
        public RelatedOrderInfoWithPriceContract StopLoss { get; set; }
    }
}
