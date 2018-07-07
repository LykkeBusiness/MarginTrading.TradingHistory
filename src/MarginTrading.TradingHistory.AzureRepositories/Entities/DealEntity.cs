using System;
using AzureStorage.Tables;
using Lykke.AzureStorage.Tables;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class DealEntity : AzureTableEntity, IDeal
    {
        public string OpenTradeId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }
        public string CloseTradeId { get; set; }
        
        public string DealId 
        {
            get => RowKey;
            set => RowKey = value;
        }
        
        public DateTime Created { get; set; }
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public decimal Volume { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal Fpl { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
