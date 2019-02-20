namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderEventWithRelatedContract : OrderEventContract
    {
        public RelatedOrderExtendedInfoContract TakeProfit { get; set; }
        
        public RelatedOrderExtendedInfoContract StopLoss { get; set; }
    }
}
