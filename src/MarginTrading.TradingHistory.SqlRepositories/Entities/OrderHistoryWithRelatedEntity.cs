using System.Linq;
using Common;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    [UsedImplicitly]
    public class OrderHistoryWithRelatedEntity : OrderHistoryEntity, IOrderHistoryWithRelated
    {
        public string TakeProfit { get; set; }

        RelatedOrderInfoWithPrice IOrderHistoryWithRelated.TakeProfit => string.IsNullOrEmpty(TakeProfit)
            ? null
            : TakeProfit.DeserializeJson<RelatedOrderInfoWithPrice[]>().FirstOrDefault();
        
        public string StopLoss { get; set; }
        
        RelatedOrderInfoWithPrice IOrderHistoryWithRelated.StopLoss=> string.IsNullOrEmpty(StopLoss)
            ? null
            : StopLoss.DeserializeJson<RelatedOrderInfoWithPrice[]>().FirstOrDefault();
    }
}