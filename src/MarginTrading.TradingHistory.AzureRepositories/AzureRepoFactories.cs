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
                    "OrdersHistory", log));
            }

            public static IPositionsHistoryRepository CreatePositionsHistoryRepository(IReloadingManager<string> connString, ILog log,
                IConvertService convertService)
            {
                return new PositionsHistoryRepository(AzureTableStorage<PositionHistoryEntity>.Create(connString, "PositionsHistory", log), 
                    convertService);
            }

            public static IDealsRepository CreateDealsHistoryRepository(IReloadingManager<string> connString, ILog log,
                IConvertService convertService)
            {
                return new DealsRepository(AzureTableStorage<DealEntity>.Create(connString, "Deals", log), 
                    convertService);
            }

            public static ITradesRepository CreateTradesHistoryRepository(IReloadingManager<string> connString, ILog log,
                IConvertService convertService)
            {
                return new TradesRepository(AzureTableStorage<TradeEntity>.Create(connString, "Trades", log), 
                    convertService);
            }
        }
    }
}
