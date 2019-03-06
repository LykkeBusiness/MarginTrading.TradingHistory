using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class OrderHistoryWithRelatedEntity : OrderHistoryEntity, IOrderHistoryWithRelated
    {
        public RelatedOrderExtendedInfo TakeProfit { get; set; }
        
        public RelatedOrderExtendedInfo StopLoss { get; set; }
    }
}