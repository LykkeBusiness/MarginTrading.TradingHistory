using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.TradeHistoryBroker
{
    public class Application : BrokerApplicationBase<PositionHistoryEvent>
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IConvertService _convertService;
        private readonly Settings _settings;

        public Application(IPositionsHistoryRepository positionsHistoryRepository, ILog logger,
            IConvertService convertService,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender)
            : base(logger, slackNotificationsSender, applicationInfo)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
            _settings = settings;
            _convertService = convertService;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.PositionsHistory.ExchangeName;
        protected override string QueuePostfix => ".PositionsHistory";

        protected override Task HandleMessage(PositionHistoryEvent positionHistoryEvent)
        {
            var position =
                _convertService.Convert<PositionContract, PositionHistory>(positionHistoryEvent.PositionSnapshot,
                    o => o.ConfigureMap(MemberList.Source));
            position.DealInfo = _convertService.Convert<DealContract, DealInfo>(positionHistoryEvent.Deal);
            position.HistoryType = positionHistoryEvent.EventType.ToType<PositionHistoryType>();
            position.HistoryTimestamp = positionHistoryEvent.Timestamp;

            return _positionsHistoryRepository.AddAsync(position);
        }
    }
}
