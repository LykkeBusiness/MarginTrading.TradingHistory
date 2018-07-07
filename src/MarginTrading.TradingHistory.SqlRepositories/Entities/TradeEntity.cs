using System;
using Common;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class TradeEntity : ITrade
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public string OrderId { get; set; }
        public string PositionId { get; set; }
        public string AssetPairId { get; set; }
        TradeType ITrade.Type => Type.ParseEnum<TradeType>();
        public string Type { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }

        public static TradeEntity Create(ITrade obj)
        {
            return new TradeEntity
            {
                Id = obj.Id,
                ClientId = obj.ClientId,
                AccountId = obj.AccountId,
                OrderId = obj.OrderId,
                PositionId = obj.PositionId,
                AssetPairId = obj.AssetPairId,
                Type = obj.Type.ToString(),
                TradeTimestamp = obj.TradeTimestamp,
                Price = obj.Price,
                Volume = obj.Volume,
            };
        }
    }
}
