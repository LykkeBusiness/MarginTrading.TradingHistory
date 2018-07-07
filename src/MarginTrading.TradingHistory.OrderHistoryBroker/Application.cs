using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Application : BrokerApplicationBase<OrderHistoryEvent>
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly ITradesRepository _tradesRepository;
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
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.OrderHistory.ExchangeName;

        protected override Task HandleMessage(OrderHistoryEvent historyEvent)
        {
            var tasks = new List<Task>();
            
            var orderHistory = historyEvent.OrderSnapshot.ToOrderHistoryDomain(historyEvent.Type);
            tasks.Add(_ordersHistoryRepository.AddAsync(orderHistory));

            if (historyEvent.Type == OrderHistoryTypeContract.Executed)
            {
                var trade = new Trade(
                    historyEvent.OrderSnapshot.Id, 
                    historyEvent.OrderSnapshot.AccountId,
                    historyEvent.OrderSnapshot.Id,
                    historyEvent.OrderSnapshot.PositionId,
                    historyEvent.OrderSnapshot.AssetPairId,
                    Core.EnumExtensions.ToType<TradeType>(historyEvent.OrderSnapshot.Direction),
                    historyEvent.OrderSnapshot.ExecutedTimestamp.Value,
                    historyEvent.OrderSnapshot.ExecutionPrice.Value,
                    historyEvent.OrderSnapshot.Volume.Value
                    );
                tasks.Add(_tradesRepository.AddAsync(trade));
            }

            return Task.WhenAll(tasks);
        }
    }
}






















