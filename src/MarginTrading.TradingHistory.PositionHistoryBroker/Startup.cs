﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using MarginTrading.TradingHistory.Services;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "PositionHistoryBroker";

        protected override void RegisterCustomServices(ContainerBuilder builder, IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterType<ConvertService>().As<IConvertService>().SingleInstance();
            
            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                throw new NotImplementedException("Azure storage is not implemented yet");
            }

            if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
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
