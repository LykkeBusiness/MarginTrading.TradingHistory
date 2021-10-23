﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using Lykke.Snow.Common.Correlation;
using MarginTrading.TradingHistory.AzureRepositories;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Services;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.TradingHistory.CorrelationBroker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "CorrelationBroker";

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddCorrelation();
        }

        public override void Configure(IApplicationBuilder app, IHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            base.Configure(app, env, appLifetime);
            app.UseCorrelation();
        }

        protected override void RegisterCustomServices(ContainerBuilder builder, IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();
            
            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                builder.RegisterInstance(AzureRepoFactories.MarginTrading.CreateCorrelationRepository(
                        settings.Nested(s => s.Db.ConnString), log, new ConvertService()))
                    .As<ICorrelationRepository>();
            }
            else if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.RegisterInstance(new CorrelationSqlRepository(settings.CurrentValue.Db.ConnString, log))
                    .As<ICorrelationRepository>();
            }
        }
    }
}