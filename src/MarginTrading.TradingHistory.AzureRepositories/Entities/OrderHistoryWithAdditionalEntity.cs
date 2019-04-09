using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class OrderHistoryWithAdditionalEntity : OrderHistoryEntity, IOrderHistoryWithAdditional
    {
        public RelatedOrderExtendedInfo TakeProfit { get; set; }
        
        public RelatedOrderExtendedInfo StopLoss { get; set; }
        
        public decimal Spread { get; set; }
        
        public decimal Commission { get; set; }
        
        public decimal OnBehalf { get; set; }
    }
}
