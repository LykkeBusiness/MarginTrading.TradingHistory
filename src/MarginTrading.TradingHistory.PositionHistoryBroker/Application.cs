// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Messaging;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Common.Correlation.RabbitMq;

using MarginTrading.Backend.Contracts.Events;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.DapperExtensions;

using Microsoft.Extensions.Logging;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    public class Application : BrokerApplicationBase<PositionHistoryEvent>
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly CorrelationContextAccessor _correlationContextAccessor;
        private readonly Settings _settings;

        static Application()
        {
            DapperHandlers.Register();
        }

        public Application(
            CorrelationContextAccessor correlationContextAccessor,
            RabbitMqCorrelationManager correlationManager,
            ILoggerFactory loggerFactory,
            IPositionsHistoryRepository positionsHistoryRepository,
            Settings settings,
            CurrentApplicationInfo applicationInfo,
            IMessagingComponentFactory<PositionHistoryEvent> messagingComponentFactory)
            : base(
                correlationManager,
                loggerFactory,
                applicationInfo,
                messagingComponentFactory)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _positionsHistoryRepository = positionsHistoryRepository;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.PositionsHistory.ExchangeName;
        public override string RoutingKey => null;
        protected override string QueuePostfix => ".PositionsHistory";

        protected override async Task HandleMessage(PositionHistoryEvent positionHistoryEvent)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                Logger.LogDebug($"Correlation id is empty for postition {positionHistoryEvent.PositionSnapshot.Id}");
            }

            var position = positionHistoryEvent
                .ToDomain()
                .AddCorrelationId(correlationId);

            var deal = (positionHistoryEvent.EventType == PositionHistoryTypeContract.Close
                        || positionHistoryEvent.EventType == PositionHistoryTypeContract.PartiallyClose)
                       && positionHistoryEvent.Deal != null
                ? Deal.FromPositionHistoryEvent(positionHistoryEvent, correlationId)
                : null;

            await _positionsHistoryRepository.AddAsync(position, deal);
        }
    }
}