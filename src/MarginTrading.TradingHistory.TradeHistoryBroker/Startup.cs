using Autofac;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.AzureRepositories;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Services;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.TradingHistory.TradeHistoryBroker
{
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "TradeHistoryBroker";

        protected override void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            
            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreateTradesRepository(
                        settings.Nested(s => s.Db.HistoryConnString), log, new ConvertService()))
                    .As<IPositionsHistoryRepository>().SingleInstance();

            }else if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.RegisterInstance(new PositionsHistorySqlRepository(
                        settings.CurrentValue.Db.ReportsSqlConnString, log))
                    .As<IPositionsHistoryRepository>();
            }
        }
    }
}
