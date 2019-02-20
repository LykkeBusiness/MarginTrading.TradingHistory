using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class OrderHistoryWithRelatedEntity : OrderHistoryEntity, IOrderHistoryWithRelated
    {
        public RelatedOrderInfoWithPrice TakeProfit { get; set; }
        
        public RelatedOrderInfoWithPrice StopLoss { get; set; }
    }
}