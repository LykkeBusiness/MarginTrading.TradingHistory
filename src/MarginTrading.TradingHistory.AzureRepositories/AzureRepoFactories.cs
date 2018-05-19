using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    public class AzureRepoFactories
    {
        public static class MarginTrading
        {

            public static OrdersHistoryRepository CreateOrdersHistoryRepository(IReloadingManager<string> connString, 
                ILog log, IConvertService convertService)
            {
                return new OrdersHistoryRepository(AzureTableStorage<OrderHistoryEntity>.Create(connString,
                    "MarginTradingOrdersHistory", log), convertService);
            }

            public static ITradesRepository CreateTradesRepository(IReloadingManager<string> connString, ILog log,
                IConvertService convertService)
            {
                return new TradesRepository(AzureTableStorage<TradeEntity>.Create(connString, "Trades", log), 
                    convertService);
            }
        }
    }
}
