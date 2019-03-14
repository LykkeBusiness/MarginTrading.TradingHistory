using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using Newtonsoft.Json;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Application : BrokerApplicationBase<OrderHistoryEvent>
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly ITradesRepository _tradesRepository;
        private readonly ILog _log;
        private readonly Settings _settings;

        public Application(IOrdersHistoryRepository ordersHistoryRepository,
            ITradesRepository tradesRepository,
            ILog logger,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender) : base(logger, slackNotificationsSender,
            applicationInfo)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
            _tradesRepository = tradesRepository;
            _log = logger;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.OrderHistory.ExchangeName;
        protected override string RoutingKey => null;

        protected override async Task HandleMessage(OrderHistoryEvent historyEvent)
        {
            var orderHistory = historyEvent.OrderSnapshot.ToOrderHistoryDomain(historyEvent.Type);
            
            try
            {
                await _ordersHistoryRepository.TryAddAsync(orderHistory);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleMessage), "Insert order history", "", ex);
                throw;
            }

            if (historyEvent.Type == OrderHistoryTypeContract.Executed)
            {
                var trade = new Trade(
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
                    historyEvent.OrderSnapshot.AdditionalInfo
                );

                try
                {
                    await _tradesRepository.AddAsync(trade);
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(HandleMessage), "Insert trade", "", ex);
                    throw;
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