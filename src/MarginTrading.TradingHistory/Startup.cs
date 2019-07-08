// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Settings;
using MarginTrading.TradingHistory.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.ApiKey;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using LogEntity = Lykke.Logs.LogEntity;

namespace MarginTrading.TradingHistory
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });
                
                var appSettings = Configuration.LoadSettings<AppSettings>(
                    throwExceptionOnCheckError: !Configuration.NotThrowExceptionsOnServiceValidation());
                
                services.AddApiKeyAuth(appSettings.CurrentValue.TradingHistoryClient);

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "TradingHistory API");
                    if (!string.IsNullOrWhiteSpace(appSettings.CurrentValue.TradingHistoryClient?.ApiKey))
                    {
                        options.OperationFilter<ApiKeyHeaderOperationFilter>();
                    }
                });

                var builder = new ContainerBuilder();

                Log = CreateLogWithSlack(Configuration, services, appSettings);

                builder.RegisterModule(new ServiceModule(appSettings.Nested(x => x.TradingHistoryService), Log));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalError(nameof(Startup), nameof(ConfigureServices), ex);
                throw;
            }
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }

                app.UseLykkeForwardedHeaders();
                
#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => ex.ToString());
#else
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse {ErrorMessage = ex.Message});
#endif
                
                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(() => CleanUp().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalError(nameof(Startup), nameof(Configure), ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet receive and process requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can receive and process requests here, so take care about it if you add logic here.

                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private static ILog CreateLogWithSlack(IConfiguration configuration, IServiceCollection services, 
            IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            #region Logs settings validation

            if (!settings.CurrentValue.TradingHistoryService.UseSerilog 
                && string.IsNullOrWhiteSpace(settings.CurrentValue.TradingHistoryService.Db.LogsConnString))
            {
                throw new Exception("Either UseSerilog must be true or LogsConnString must be set");
            }

            #endregion Logs settings validation

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            ILykkeLogToAzureSlackNotificationsManager slackNotificationsManager = null;
            if (settings.CurrentValue.SlackNotifications != null)
            {
                var slackService = services.UseSlackNotificationsSenderViaAzureQueue(
                    new Lykke.AzureQueueIntegration.AzureQueueSettings
                    {
                        ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                        QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
                    }, aggregateLogger);

                slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);
            }

            if (settings.CurrentValue.TradingHistoryService.UseSerilog)
            {
                aggregateLogger.AddLog(new SerilogLogger(typeof(Startup).Assembly, configuration));
            }
            else if (settings.CurrentValue.TradingHistoryService.Db.StorageMode == StorageMode.Azure)
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(settings.Nested(x => x.TradingHistoryService.Db.LogsConnString), 
                        "TradingHistoryServiceLog", consoleLogger),
                    consoleLogger);
                
                // Creating azure storage logger, which logs own messages to concole log
                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }
            else if (settings.CurrentValue.TradingHistoryService.Db.StorageMode == StorageMode.SqlServer)
            {
                var sqlLogger = new LogToSql(new SqlLogRepository("TradingHistoryAPIsLog",
                    settings.CurrentValue.TradingHistoryService.Db.LogsConnString));
                
                aggregateLogger.AddLog(sqlLogger);
            }

            LogLocator.Log = aggregateLogger;
            
            return aggregateLogger;
        }
    }
}
