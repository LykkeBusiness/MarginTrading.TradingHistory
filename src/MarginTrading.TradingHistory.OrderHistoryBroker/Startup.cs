using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.AzureRepositories;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "OrderHistoryBroker";

        protected override void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, IReloadingManager<Settings> settings, ILog log, bool isLive)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            
            var repositories = new List<IOrdersHistoryRepository>();
            if (settings.CurrentValue.Db.HistoryConnString != null)
            {
                repositories.Add(
                    AzureRepoFactories.MarginTrading.CreateOrdersHistoryRepository(
                        settings.Nested(s => s.Db.HistoryConnString), log));
            }

            if (settings.CurrentValue.Db.ReportsSqlConnString != null)
            {
                repositories.Add(
                    new SqlRepositories.OrdersHistorySqlRepository(settings.CurrentValue.Db.ReportsSqlConnString,
                        log));
            }
            
            builder.RegisterInstance(new RepositoryAggregator(repositories)).As<IOrdersHistoryRepository>(); 
        }
    }
}
