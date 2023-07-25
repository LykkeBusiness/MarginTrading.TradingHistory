// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Settings.ServiceSettings;
using MarginTrading.TradingHistory.Services;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarginTrading.TradingHistory.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<TradingHistorySettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<TradingHistorySettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            // TODO: Add your dependencies here

            var convertService = new ConvertService();

            if (_settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
               throw new NotImplementedException("Azure storage is not implemented yet");
            }

            if (_settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.Register(ctx => new OrdersHistorySqlRepository(
                        _settings.CurrentValue.Db.HistoryConnString,
                        ctx.Resolve<ILogger<OrdersHistorySqlRepository>>(),
                        _settings.CurrentValue.Db.OrderBlotterExecutionTimeout))
                    .As<IOrdersHistoryRepository>()
                    .SingleInstance();
                
                builder.Register(ctx => new PositionsHistorySqlRepository(
                        _settings.CurrentValue.Db.HistoryConnString,
                        ctx.Resolve<ILogger<PositionsHistorySqlRepository>>()))
                    .As<IPositionsHistoryRepository>()
                    .SingleInstance();
                
                builder.Register(ctx => new TradesSqlRepository(
                        _settings.CurrentValue.Db.HistoryConnString, 
                        ctx.Resolve<ILogger<TradesSqlRepository>>()))
                    .As<ITradesRepository>()
                    .SingleInstance();
                
                builder.Register(ctx => new DealsSqlRepository(
                        _settings.CurrentValue.Db.HistoryConnString,
                        ctx.Resolve<ILogger<DealsSqlRepository>>()))
                    .As<IDealsRepository>()
                    .SingleInstance();

                builder.RegisterInstance(new OrderHistoryForSupportQuery(_settings.CurrentValue.Db.HistoryConnString, _settings.CurrentValue.Db.OrderHistoryForSupportExecutionTimeout));
            }

            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();

            builder.Populate(_services);
        }
    }
}
