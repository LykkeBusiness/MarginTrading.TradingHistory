using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class TradeEntity : AzureTableEntity, ITrade
    {
        public TradeEntity()
        {
            RowKey = GetRowKey();
        }

        public string Id
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string OrderId { get; set; }
        public string PositionId { get; set; }
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public string AssetPairId { get; set; }
        public TradeType Type { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }

        public static string GetPartitionKey(string id)
        {
            return id;
        }

        public static string GetRowKey()
        {
            return "-";
        }
    }
}
