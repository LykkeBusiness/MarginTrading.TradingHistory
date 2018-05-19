using System;
using Common;
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
        TradeType ITrade.Type => this.Type.ParseEnum<TradeType>();
        public string Type { get; set; }
        decimal ITrade.Price => (decimal) Price;
        public double Price { get; set; }
        decimal ITrade.Volume => (decimal) Volume;
        public double Volume { get; set; }

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
