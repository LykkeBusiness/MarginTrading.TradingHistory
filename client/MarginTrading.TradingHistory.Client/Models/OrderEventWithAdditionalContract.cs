namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderEventWithAdditionalContract : OrderEventContract
    {
        public RelatedOrderExtendedInfoContract TakeProfit { get; set; }
        
        public RelatedOrderExtendedInfoContract StopLoss { get; set; }
        
        public decimal Spread { get; set; }
        
        public decimal Commission { get; set; }
        
        public decimal OnBehalf { get; set; }
    }
}
