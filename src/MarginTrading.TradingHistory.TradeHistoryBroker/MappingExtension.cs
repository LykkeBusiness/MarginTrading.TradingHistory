using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.TradeHistoryBroker
{
    public static class MappingExtension
    {
        public static Trade ToTradeDomain(this TradeContract src)
        {
            return new Trade
            {
                Id = src.Id,
                ClientId = src.ClientId,
                AccountId = src.AccountId,
                OrderId = src.OrderId,
                PositionId = src.PositionId,
                AssetPairId = src.AssetPairId,
                Type = src.Type.ToType<TradeType>(),
                TradeTimestamp = src.Timestamp,
                Price = src.Price,
                Volume = src.Volume,
            };
        }
    }
}
