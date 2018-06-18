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
                    "OrdersHistory", log), convertService);
            }

            public static IPositionsHistoryRepository CreateTradesRepository(IReloadingManager<string> connString, ILog log,
                IConvertService convertService)
            {
                return new PositionsHistoryRepository(AzureTableStorage<PositionHistoryEntity>.Create(connString, "PositionsHistory", log), 
                    convertService);
            }
        }
    }
}
