using System;
using Common;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class TradeEntity : ITrade
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string OrderId { get; set; }
        public string AssetPairId { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        OrderType ITrade.OrderType => OrderType.ParseEnum<OrderType>();
        public string OrderType { get; set; }
        TradeType ITrade.Type => Type.ParseEnum<TradeType>();
        public string Type { get; set; }
        OriginatorType ITrade.Originator => Originator.ParseEnum<OriginatorType>();
        public string Originator { get; set; }
        public DateTime TradeTimestamp { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public decimal? OrderExpectedPrice { get; set; }
        public decimal FxRate { get; set; }
        public string AdditionalInfo { get; set; }
        public string CancelledBy { get; set; }

        public static TradeEntity Create(ITrade obj)
        {
            return new TradeEntity
            {
                Id = obj.Id,
                AccountId = obj.AccountId,
                OrderId = obj.OrderId,
                AssetPairId = obj.AssetPairId,
                OrderCreatedDate = obj.OrderCreatedDate,
                OrderType = obj.OrderType.ToString(),
                Type = obj.Type.ToString(),
                Originator = obj.Originator.ToString(),
                TradeTimestamp = obj.TradeTimestamp,
                Price = obj.Price,
                Volume = obj.Volume,
                OrderExpectedPrice = obj.OrderExpectedPrice,
                FxRate = obj.FxRate,
                AdditionalInfo = obj.AdditionalInfo,
                CancelledBy = obj.CancelledBy
            };
        }
    }
}
