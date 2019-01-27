using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class TradeEntity : AzureTableEntity, ITrade
    {
        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }
        
        public string Id 
        {
            get => RowKey;
            set => RowKey = value;
        }
        
        public string OrderId { get; set; }
        public string AssetPairId { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        public OrderType OrderType { get; set; }
        public TradeType Type { get; set; }
        public OriginatorType Originator { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public decimal? OrderExpectedPrice { get; set; }
        public decimal FxRate { get; set; }
        public string AdditionalInfo { get; set; }
        public string CancelledBy { get; set; }
    }
}
