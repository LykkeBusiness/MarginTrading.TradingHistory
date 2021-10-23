// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using Lykke.Snow.Common.Correlation.RabbitMq;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.Extensions.Logging;
using CorrelationEntityType = MarginTrading.TradingHistory.Core.Domain.CorrelationEntityType;

namespace MarginTrading.TradingHistory.CorrelationBroker
{
    public class Application : BrokerApplicationBase<CorrelationContract>
    {
        private readonly ICorrelationRepository _correlationRepository;
        private readonly Settings _settings;
        private readonly RabbitMqCorrelationManager _correlationManager;
        private readonly ILog _log;

        public Application(
            RabbitMqCorrelationManager correlationManager,
            ILoggerFactory loggerFactory, 
            ICorrelationRepository correlationRepository, 
            ILog logger,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender)
            : base(loggerFactory, logger, slackNotificationsSender, applicationInfo)
        {
            _correlationManager = correlationManager;
            _correlationRepository = correlationRepository;
            _settings = settings;
            _log = logger;
        }

        protected override Action<IDictionary<string, object>> ReadHeadersAction =>
            _correlationManager.FetchCorrelationIfExists;

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.Correlation.ExchangeName;
        public override string RoutingKey => null;
        protected override string QueuePostfix => ".Correlation";

        protected override async Task HandleMessage(CorrelationContract correlation)
        {
            await _log.WriteInfoAsync(nameof(Application), nameof(HandleMessage), $"Received correlation: {correlation.ToJson()}");
            
            var entity = new Correlation(
                Guid.NewGuid().ToString("N"),
                correlation.CorrelationId,
                EnumExtensions.ToType<CorrelationEntityType>(correlation.EntityType),
                correlation.EntityId,
                correlation.Timestamp);
            
            await _correlationRepository.AddAsync(entity);
        }
    }
}
