// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Common.Correlation.RabbitMq;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.Extensions.Logging;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    public class Application : BrokerApplicationBase<PositionHistoryEvent>
    {
        private readonly IPositionsHistoryRepository _positionsHistoryRepository;
        private readonly IDealsRepository _dealsRepository;
        private readonly IConvertService _convertService;
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly CorrelationContextAccessor _correlationContextAccessor;

        public Application(
            CorrelationContextAccessor correlationContextAccessor,
            RabbitMqCorrelationManager correlationManager,
            ILoggerFactory loggerFactory, 
            IPositionsHistoryRepository positionsHistoryRepository, 
            IDealsRepository dealsRepository,
            ILogger<Application> logger,
            IConvertService convertService,
            Settings settings, 
            CurrentApplicationInfo applicationInfo)
            : base(correlationManager, loggerFactory, logger, applicationInfo)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _positionsHistoryRepository = positionsHistoryRepository;
            _dealsRepository = dealsRepository;
            _logger = logger;
            _settings = settings;
            _convertService = convertService;
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
                _logger.LogDebug($"Correlation id is empty for postition {positionHistoryEvent.PositionSnapshot.Id}");
            }
            
            var position = Map(positionHistoryEvent, correlationId);
            
            var deal = (positionHistoryEvent.EventType == PositionHistoryTypeContract.Close
                        || positionHistoryEvent.EventType == PositionHistoryTypeContract.PartiallyClose)
                       && positionHistoryEvent.Deal != null
                ? new Deal(
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
                    additionalInfo: positionHistoryEvent.Deal.AdditionalInfo,
                    pnlOfTheLastDay: positionHistoryEvent.Deal.PnlOfTheLastDay,
                    correlationId: correlationId
                )
                : null;
            
            await _positionsHistoryRepository.AddAsync(position, deal);
        }

        private static PositionHistory Map(PositionHistoryEvent positionHistoryEvent, string correlationId)
        {
            return new PositionHistory
            {
                Id = positionHistoryEvent.PositionSnapshot.Id,
                DealId = positionHistoryEvent.Deal?.DealId,
                Code = positionHistoryEvent.PositionSnapshot.Code,
                AssetPairId = positionHistoryEvent.PositionSnapshot.AssetPairId,
                Direction = positionHistoryEvent.PositionSnapshot.Direction.ToType<PositionDirection>(),
                Volume = positionHistoryEvent.PositionSnapshot.Volume,
                AccountId = positionHistoryEvent.PositionSnapshot.AccountId,
                TradingConditionId = positionHistoryEvent.PositionSnapshot.TradingConditionId,
                AccountAssetId = positionHistoryEvent.PositionSnapshot.AccountAssetId,
                ExpectedOpenPrice = positionHistoryEvent.PositionSnapshot.ExpectedOpenPrice,
                OpenMatchingEngineId = positionHistoryEvent.PositionSnapshot.OpenMatchingEngineId,
                OpenDate = positionHistoryEvent.PositionSnapshot.OpenDate,
                OpenTradeId = positionHistoryEvent.PositionSnapshot.OpenTradeId,
                OpenOrderType = positionHistoryEvent.PositionSnapshot.OpenOrderType.ToType<OrderType>(),
                OpenOrderVolume = positionHistoryEvent.PositionSnapshot.OpenOrderVolume,
                OpenPrice = positionHistoryEvent.PositionSnapshot.OpenPrice,
                OpenFxPrice = positionHistoryEvent.PositionSnapshot.OpenFxPrice,
                EquivalentAsset = positionHistoryEvent.PositionSnapshot.EquivalentAsset,
                OpenPriceEquivalent = positionHistoryEvent.PositionSnapshot.OpenPriceEquivalent,
                RelatedOrders = positionHistoryEvent.PositionSnapshot.RelatedOrders.Select(ro => new RelatedOrderInfo
                {
                    Type = ro.Type.ToType<OrderType>(),
                    Id = ro.Id,
                }).ToList(),
                LegalEntity = positionHistoryEvent.PositionSnapshot.LegalEntity,
                OpenOriginator = positionHistoryEvent.PositionSnapshot.OpenOriginator.ToType<OriginatorType>(),
                ExternalProviderId = positionHistoryEvent.PositionSnapshot.ExternalProviderId,
                SwapCommissionRate = positionHistoryEvent.PositionSnapshot.SwapCommissionRate,
                OpenCommissionRate = positionHistoryEvent.PositionSnapshot.OpenCommissionRate,
                CloseCommissionRate = positionHistoryEvent.PositionSnapshot.CloseCommissionRate,
                CommissionLot = positionHistoryEvent.PositionSnapshot.CommissionLot,
                CloseMatchingEngineId = positionHistoryEvent.PositionSnapshot.CloseMatchingEngineId,
                ClosePrice = positionHistoryEvent.PositionSnapshot.ClosePrice,
                CloseFxPrice = positionHistoryEvent.PositionSnapshot.CloseFxPrice,
                ClosePriceEquivalent = positionHistoryEvent.PositionSnapshot.ClosePriceEquivalent,
                StartClosingDate = positionHistoryEvent.PositionSnapshot.StartClosingDate,
                CloseDate = positionHistoryEvent.PositionSnapshot.CloseDate,
                CloseOriginator = positionHistoryEvent.PositionSnapshot.CloseOriginator?.ToType<OriginatorType>(),
                CloseReason = positionHistoryEvent.PositionSnapshot.CloseReason.ToType<OrderCloseReason>(),
                CloseComment = positionHistoryEvent.PositionSnapshot.CloseComment,
                CloseTrades = positionHistoryEvent.PositionSnapshot.CloseTrades,
                FxAssetPairId = positionHistoryEvent.PositionSnapshot.FxAssetPairId,
                FxToAssetPairDirection = positionHistoryEvent.PositionSnapshot.FxToAssetPairDirection.ToType<FxToAssetPairDirection>(),
                LastModified = positionHistoryEvent.PositionSnapshot.LastModified,
                TotalPnL = positionHistoryEvent.PositionSnapshot.TotalPnL,
                ChargedPnl = positionHistoryEvent.PositionSnapshot.ChargedPnl,
                AdditionalInfo = positionHistoryEvent.PositionSnapshot.AdditionalInfo,
                HistoryType = positionHistoryEvent.EventType.ToType<PositionHistoryType>(),
                HistoryTimestamp = positionHistoryEvent.Timestamp,
                ForceOpen = positionHistoryEvent.PositionSnapshot.ForceOpen,
                CorrelationId = correlationId
            };
        }
    }
}
