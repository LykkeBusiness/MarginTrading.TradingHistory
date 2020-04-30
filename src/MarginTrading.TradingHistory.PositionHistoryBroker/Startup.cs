// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.AzureRepositories;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Services;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "PositionHistoryBroker";

        protected override void RegisterCustomServices(ContainerBuilder builder, IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();
            
            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreatePositionsHistoryRepository(
                        settings.Nested(s => s.Db.ConnString), log, new ConvertService()))
                    .As<IPositionsHistoryRepository>().SingleInstance();
                builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreateDealsHistoryRepository(
                        settings.Nested(s => s.Db.ConnString), log, new ConvertService()))
                    .As<IDealsRepository>().SingleInstance();

            }
            else if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.RegisterInstance(new PositionsHistorySqlRepository(
                        settings.CurrentValue.Db.ConnString, log))
                    .As<IPositionsHistoryRepository>();
                builder.RegisterInstance(new DealsSqlRepository(
                        settings.CurrentValue.Db.ConnString, log))
                    .As<IDealsRepository>();
            }
        }
    }
}
