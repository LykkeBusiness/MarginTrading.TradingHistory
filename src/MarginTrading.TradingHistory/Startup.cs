// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Settings;
using MarginTrading.TradingHistory.Modules;
using Lykke.SettingsReader;
using Lykke.SettingsReader.ConfigurationProvider;
using Lykke.SettingsReader.SettingsTemplate;
using Lykke.Snow.Common.AssemblyLogging;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Common.Correlation.Cqrs;
using Lykke.Snow.Common.Correlation.Http;
using Lykke.Snow.Common.Correlation.RabbitMq;
using Lykke.Snow.Common.Correlation.Serilog;
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
using Microsoft.Extensions.Logging;
using Lykke.Snow.Common.Startup.Hosting;
using Lykke.Snow.Common.Startup.Log;
using Microsoft.Extensions.Hosting;
using Serilog.Core;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace MarginTrading.TradingHistory
{
    public class Startup
    {
        private IReloadingManager<AppSettings> _mtSettingsManager;
        public IHostEnvironment Environment { get; }
        public ILifetimeScope ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;

        public Startup(IHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddHttpSourceConfiguration()
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddAssemblyLogger();
                services
                    .AddControllers()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });

                _mtSettingsManager = Configuration.LoadSettings<AppSettings>(
                    throwExceptionOnCheckError: !Configuration.NotThrowExceptionsOnServiceValidation());

                services.AddApiKeyAuth(_mtSettingsManager.CurrentValue.TradingHistoryClient);

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "TradingHistory API");
                    if (!string.IsNullOrWhiteSpace(_mtSettingsManager.CurrentValue.TradingHistoryClient?.ApiKey))
                    {
                        options.AddApiKeyAwareness();
                    }
                });

                var correlationContextAccessor = new CorrelationContextAccessor();
                services.AddSingleton<CorrelationContextAccessor>();
                services.AddSingleton<RabbitMqCorrelationManager>();
                services.AddSingleton<CqrsCorrelationManager>();
                services.AddTransient<HttpCorrelationHandler>();

                Log = CreateLog(Configuration, services, _mtSettingsManager, correlationContextAccessor);

                services.AddSingleton<ILoggerFactory>(x => new WebHostLoggerFactory(Log));

                services.AddSettingsTemplateGenerator();
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
                ApplicationContainer = app.ApplicationServices.GetAutofacRoot();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }

                app.UseLykkeForwardedHeaders();

                app.UseCorrelation();
#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse { ErrorMessage = ex.Message }, false, false);
#else
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse { ErrorMessage = "Technical Problem" }, false, false);
#endif
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.AddSettingsTemplateEndpoint();
                });
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) =>
                        swagger.Servers = new List<OpenApiServer> {
                            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                        });
                });
                app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading Engine API Swagger"));
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(() => CleanUp().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalError(nameof(Startup), nameof(Configure), ex);
                throw;
            }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ServiceModule(_mtSettingsManager.Nested(x => x.TradingHistoryService), Log));
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet receive and process requests here
                Program.AppHost.WriteLogs(Environment, LogLocator.Log);

                await Log.WriteMonitorAsync("", $"Env: {Program.EnvInfo}", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
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

        private static ILog CreateLog(IConfiguration configuration, IServiceCollection services,
            IReloadingManager<AppSettings> settings, CorrelationContextAccessor correlationContextAccessor)
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

            if (settings.CurrentValue.TradingHistoryService.UseSerilog)
            {
                aggregateLogger.AddLog(new SerilogLogger(typeof(Startup).Assembly, configuration, new List<ILogEventEnricher>()
                {
                    new CorrelationLogEventEnricher("CorrelationId", correlationContextAccessor)
                }));
            }
            else if (settings.CurrentValue.TradingHistoryService.Db.StorageMode == StorageMode.Azure)
            {
                throw new NotSupportedException("Azure storage is not supported for logs");
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