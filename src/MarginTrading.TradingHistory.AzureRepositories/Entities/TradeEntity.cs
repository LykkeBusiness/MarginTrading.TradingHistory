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
        public string PositionId { get; set; }
        public string AssetPairId { get; set; }
        public TradeType Type { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }
}
