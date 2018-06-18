using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Services;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MarginTrading.TradingHistory.BrokerBase
{
    public abstract class BrokerStartupBase<TApplicationSettings, TSettings>
        where TApplicationSettings : class, IBrokerApplicationSettings<TSettings>
        where TSettings: BrokerSettingsBase
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public ILog Log { get; private set; }

        protected abstract string ApplicationName { get; }

        protected BrokerStartupBase(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddDevJson(env)
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Error)
                .AddDebug(LogLevel.Warning);

            services.AddSingleton(loggerFactory);
            services.AddLogging();
            services.AddSingleton(Configuration);
            services.AddMvc();

            var applicationSettings = Configuration.LoadSettings<TApplicationSettings>()
                .Nested(s =>
                {
                    var settings = s.MtBackend.MarginTradingLive;
                    if (!string.IsNullOrEmpty(Configuration["Env"]))
                    {
                        settings.Env = Configuration["Env"];
                    }
                    SetSettingValues(settings, Configuration);
                    return s;
                });

            var builder = new ContainerBuilder();
            RegisterServices(services, applicationSettings, builder);
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        protected virtual void SetSettingValues(TSettings source, IConfigurationRoot configuration)
        {
            //if needed TSetting properties may be set
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            app.UseMvc();

            var applications = app.ApplicationServices.GetServices<IBrokerApplication>();

            appLifetime.ApplicationStarted.Register(async () =>
            {
                foreach (var application in applications)
                {
                    application.Run();
                }
                
                await Log.WriteMonitorAsync("", "", $"Started");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                foreach (var application in applications)
                {
                    application.StopApplication();
                }
            });

            appLifetime.ApplicationStopped.Register(async () =>
            {
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", $"Terminating");
                }
                
                ApplicationContainer.Dispose();
            });
        }

        protected abstract void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, IReloadingManager<TSettings> settings, ILog log);

        protected virtual ILog CreateLogWithSlack(IServiceCollection services,
            IReloadingManager<TApplicationSettings> settings, CurrentApplicationInfo applicationInfo)
        {
            var logToConsole = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(logToConsole);

            ISlackNotificationsSender slackNotificationsSender = null;
            if (settings.CurrentValue.SlackNotifications != null)
            {
                slackNotificationsSender = services.UseSlackNotificationsSenderViaAzureQueue(
                    settings.CurrentValue.SlackNotifications.AzureQueue, aggregateLogger);
            }

            var slackService =
                new MtSlackNotificationsSender(slackNotificationsSender, ApplicationName, applicationInfo.EnvInfo);

            services.AddSingleton<ISlackNotificationsSender>(slackService);

            // Creating azure storage logger, which logs own messages to concole log
            var dbLogConnectionString = settings.CurrentValue.MtBrokersLogs?.DbConnString;
            if (!string.IsNullOrEmpty(dbLogConnectionString) &&
                !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var logTableName = ApplicationName + applicationInfo.EnvInfo + "Log";
                
                if (settings.CurrentValue.MtBrokersLogs?.StorageMode == StorageMode.Azure)
                {
                    var logToAzureStorage = services.UseLogToAzureStorage(
                        settings.Nested(s => s.MtBrokersLogs.DbConnString), slackService,
                        logTableName, aggregateLogger);

                    aggregateLogger.AddLog(logToAzureStorage);
                }
                else if (settings.CurrentValue.MtBrokersLogs?.StorageMode == StorageMode.SqlServer)
                {
                    var sqlLogger = new LogToSql(new SqlLogRepository(logTableName, dbLogConnectionString));
                
                    aggregateLogger.AddLog(sqlLogger);
                }
            }

            return aggregateLogger;
        }


        private void RegisterServices(IServiceCollection services, IReloadingManager<TApplicationSettings> applicationSettings,
            ContainerBuilder builder)
        {
            var applicationInfo = new CurrentApplicationInfo(PlatformServices.Default.Application.ApplicationVersion,
                ApplicationName);
            builder.RegisterInstance(applicationInfo).AsSelf().SingleInstance();
            Log = CreateLogWithSlack(services, applicationSettings, applicationInfo);
            builder.RegisterInstance(Log).As<ILog>().SingleInstance();
            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();
            builder.RegisterInstance(applicationSettings).AsSelf().SingleInstance();

            var settings = applicationSettings.Nested(s => s.MtBackend.MarginTradingLive);
            builder.RegisterInstance(settings).AsSelf().SingleInstance();
            builder.RegisterInstance(settings.CurrentValue).AsSelf().SingleInstance();

            RegisterCustomServices(services, builder, settings, Log);
            builder.Populate(services);
        }
    }
}
