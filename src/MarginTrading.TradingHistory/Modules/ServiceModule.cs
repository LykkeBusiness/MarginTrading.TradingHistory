using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Settings.ServiceSettings;
using MarginTrading.TradingHistory.Services;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.AzureRepositories;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

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

            builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreateOrdersHistoryRepository(
                    _settings.Nested(s => s.Db.HistoryConnString), _log))
                .As<IOrdersHistoryRepository>()
                .SingleInstance();
            
            builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreateTradesRepository(
                    _settings.Nested(s => s.Db.HistoryConnString), _log, new ConvertService()))
                .As<ITradesRepository>()
                .SingleInstance();

            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();

            builder.Populate(_services);
        }
    }
}
