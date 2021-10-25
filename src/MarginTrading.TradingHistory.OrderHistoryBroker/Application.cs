// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Common.Correlation.RabbitMq;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Application : BrokerApplicationBase<OrderHistoryEvent>
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly ITradesRepository _tradesRepository;
        private readonly ILog _log;
        private readonly Settings _settings;
        private readonly CorrelationContextAccessor _correlationContextAccessor;

        public Application(
            CorrelationContextAccessor correlationContextAccessor,
            RabbitMqCorrelationManager correlationManager,
            ILoggerFactory loggerFactory, 
            IOrdersHistoryRepository ordersHistoryRepository,
            ITradesRepository tradesRepository,
            ILog logger,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender) : base(correlationManager,
            loggerFactory, logger, slackNotificationsSender, applicationInfo)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _ordersHistoryRepository = ordersHistoryRepository;
            _tradesRepository = tradesRepository;
            _log = logger;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.OrderHistory.ExchangeName;
        public override string RoutingKey => null;

        protected override async Task HandleMessage(OrderHistoryEvent historyEvent)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                await _log.WriteMonitorAsync(
                    nameof(HandleMessage), 
                    nameof(OrderHistoryEvent),
                    $"Correlation id is empty for order {historyEvent.OrderSnapshot.Id}");
            }

            var orderHistory = historyEvent.OrderSnapshot.ToOrderHistoryDomain(historyEvent.Type, correlationId);

            var trade = historyEvent.Type == OrderHistoryTypeContract.Executed
                ? new Trade(
                    historyEvent.OrderSnapshot.Id,
                    historyEvent.OrderSnapshot.AccountId,
                    historyEvent.OrderSnapshot.Id,
                    historyEvent.OrderSnapshot.AssetPairId,
                    historyEvent.OrderSnapshot.CreatedTimestamp,
                    Core.EnumExtensions.ToType<OrderType>(historyEvent.OrderSnapshot.Type),
                    Core.EnumExtensions.ToType<TradeType>(historyEvent.OrderSnapshot.Direction),
                    Core.EnumExtensions.ToType<OriginatorType>(historyEvent.OrderSnapshot.Originator),
                    historyEvent.OrderSnapshot.ExecutedTimestamp.Value,
                    historyEvent.OrderSnapshot.ExecutionPrice.Value,
                    historyEvent.OrderSnapshot.Volume.Value,
                    historyEvent.OrderSnapshot.ExpectedOpenPrice,
                    historyEvent.OrderSnapshot.FxRate,
                    historyEvent.OrderSnapshot.AdditionalInfo,
                    historyEvent.OrderSnapshot.ExternalOrderId,
                    correlationId
                )
                : null;
            
            await _ordersHistoryRepository.AddAsync(orderHistory, trade);

            if (trade == null)
            {
                return;
            }
            
            var cancelledTradeId = TryGetCancelledTradeId(historyEvent.OrderSnapshot);

            if (!string.IsNullOrEmpty(cancelledTradeId))
            {
                try
                {
                    await _tradesRepository.SetCancelledByAsync(cancelledTradeId, 
                        historyEvent.OrderSnapshot.Id);
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(HandleMessage), "SetCancelledByAsync", "", ex);
                }
            }
        }

        private string TryGetCancelledTradeId(OrderContract order)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<Dictionary<string, object>>(order.AdditionalInfo);

                if (info.TryGetValue(_settings.IsCancellationTradeAttributeName, out var cancellationFlagStr))
                {
                    if (bool.TryParse(cancellationFlagStr.ToString(), out var cancellationFlag))
                    {
                        if (cancellationFlag &&
                            info.TryGetValue(_settings.CancelledTradeIdAttributeName, out var cancelledTradeId))
                        {
                            return cancelledTradeId.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteWarningAsync(nameof(TryGetCancelledTradeId), order.AdditionalInfo,
                    "Error getting of cancelled trade id", ex);
            }

            return null;
        }
    }
}
