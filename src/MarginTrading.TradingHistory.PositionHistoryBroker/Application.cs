using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    public class Application : BrokerApplicationBase<PositionHistoryEvent>
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IDealsRepository _dealsRepository;
        private readonly IConvertService _convertService;
        private readonly ILog _log;
        private readonly Settings _settings;

        public Application(IPositionsHistoryRepository positionsHistoryRepository, 
            IDealsRepository dealsRepository,
            ILog logger,
            IConvertService convertService,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender)
            : base(logger, slackNotificationsSender, applicationInfo)
        {
            _positionsHistoryRepository = positionsHistoryRepository;
            _dealsRepository = dealsRepository;
            _log = logger;
            _settings = settings;
            _convertService = convertService;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.PositionsHistory.ExchangeName;
        protected override string RoutingKey => null;
        protected override string QueuePostfix => ".PositionsHistory";

        protected override Task HandleMessage(PositionHistoryEvent positionHistoryEvent)
        {
            var tasks = new List<Task>();
            
            var position = _convertService.Convert<PositionContract, PositionHistory>(
                    positionHistoryEvent.PositionSnapshot, o => o.ConfigureMap(MemberList.Source));
            position.HistoryType = positionHistoryEvent.EventType.ToType<PositionHistoryType>();
            position.HistoryTimestamp = positionHistoryEvent.Timestamp;
            position.DealId = positionHistoryEvent.Deal?.DealId;
            
            tasks.Add(_positionsHistoryRepository.AddAsync(position));

            if ((positionHistoryEvent.EventType == PositionHistoryTypeContract.Close
                 || positionHistoryEvent.EventType == PositionHistoryTypeContract.PartiallyClose)
                && positionHistoryEvent.Deal != null)
            {
                var deal = new Deal(
                    dealId: positionHistoryEvent.Deal.DealId,
                    created: positionHistoryEvent.Deal.Created,
                    accountId: positionHistoryEvent.PositionSnapshot.AccountId,
                    assetPairId: positionHistoryEvent.PositionSnapshot.AssetPairId,
                    openTradeId: positionHistoryEvent.Deal.OpenTradeId,
                    openOrderType: positionHistoryEvent.Deal.OpenOrderType.ToType<OrderType>(),
                    openOrderVolume: positionHistoryEvent.Deal.OpenOrderVolume,
                    openOrderExpectedPrice: positionHistoryEvent.Deal.OpenOrderExpectedPrice,
                    closeTradeId: positionHistoryEvent.Deal.CloseTradeId,
                    closeOrderType: positionHistoryEvent.Deal.CloseOrderType.ToType<OrderType>(),
                    closeOrderVolume: positionHistoryEvent.Deal.CloseOrderVolume,
                    closeOrderExpectedPrice: positionHistoryEvent.Deal.CloseOrderExpectedPrice,
                    direction: positionHistoryEvent.PositionSnapshot.Direction.ToType<PositionDirection>(),
                    volume: positionHistoryEvent.Deal.Volume,
                    originator: positionHistoryEvent.Deal.Originator.ToType<OriginatorType>(),
                    openPrice: positionHistoryEvent.Deal.OpenPrice,
                    openFxPrice: positionHistoryEvent.Deal.OpenFxPrice,
                    closePrice: positionHistoryEvent.Deal.ClosePrice,
                    closeFxPrice: positionHistoryEvent.Deal.CloseFxPrice,
                    fpl: positionHistoryEvent.Deal.Fpl,
                    additionalInfo: positionHistoryEvent.Deal.AdditionalInfo);
                tasks.Add(_dealsRepository.AddAsync(deal));
            }

            return Task.WhenAll(tasks.Select(t => Task.Run(async () =>
            {
                try
                {
                    await t;
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(HandleMessage), "SwitchThread", "", ex);
                }
            })));
        }
    }
}
