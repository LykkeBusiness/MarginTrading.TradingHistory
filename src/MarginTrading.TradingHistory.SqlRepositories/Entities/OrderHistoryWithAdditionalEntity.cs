using System.Linq;
using Common;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    [UsedImplicitly]
    public class OrderHistoryWithAdditionalEntity : OrderHistoryEntity, IOrderHistoryWithAdditional
    {
        public string TakeProfit { get; set; }

        RelatedOrderExtendedInfo IOrderHistoryWithAdditional.TakeProfit => string.IsNullOrEmpty(TakeProfit)
            ? null
            : TakeProfit.DeserializeJson<RelatedOrderExtendedInfo[]>().FirstOrDefault();
        
        public string StopLoss { get; set; }
       
        RelatedOrderExtendedInfo IOrderHistoryWithAdditional.StopLoss=> string.IsNullOrEmpty(StopLoss)
            ? null
            : StopLoss.DeserializeJson<RelatedOrderExtendedInfo[]>().FirstOrDefault();
        
        
        public decimal Spread { get; set; }
        
        public decimal Commission { get; set; }
        
        public decimal OnBehalf { get; set; }
    }
}
