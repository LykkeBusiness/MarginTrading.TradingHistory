// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using Lykke.SettingsReader.SettingsTemplate;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostEnvironment env, IConfiguration configuration) : base(env, configuration)
        {
        }

        protected override string ApplicationName => "OrderHistoryBroker";

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSettingsTemplateGenerator();
        }

        protected override void ConfigureEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapSettingsTemplate();
        }

        protected override void RegisterCustomServices(ContainerBuilder builder, IReloadingManager<Settings> settings)
        {
            builder.AddJsonBrokerMessagingFactory<OrderHistoryEvent>();
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();

            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                throw new NotImplementedException("Azure storage is not implemented yet");
            }

            if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.Register(c => new SqlRepositories.OrdersHistorySqlRepository(
                        settings.CurrentValue.Db.ConnString, c.Resolve<ILogger<SqlRepositories.OrdersHistorySqlRepository>>()))
                    .As<IOrdersHistoryRepository>()
                    .SingleInstance();
                builder.Register(c => new SqlRepositories.TradesSqlRepository(
                        settings.CurrentValue.Db.ConnString, c.Resolve<ILogger<SqlRepositories.TradesSqlRepository>>()))
                    .As<ITradesRepository>()
                    .SingleInstance();
            }
        }
    }
}