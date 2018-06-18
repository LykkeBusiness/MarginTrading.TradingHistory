using System.Threading.Tasks;
using Common.Log;
using Lykke.SlackNotifications;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core.Repositories;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Application : BrokerApplicationBase<OrderHistoryEvent>
    {
        private readonly IOrdersHistoryRepository[] _ordersHistoryRepositories;
        private readonly Settings _settings;

        public Application(IOrdersHistoryRepository[] ordersHistoryRepositories, ILog logger,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender) : base(logger, slackNotificationsSender,
            applicationInfo)
        {
            _ordersHistoryRepositories = ordersHistoryRepositories;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.OrderHistory.ExchangeName;

        protected override async Task HandleMessage(OrderHistoryEvent historyEvent)
        {
            var orderHistory = historyEvent.OrderSnapshot.ToOrderHistoryDomain(historyEvent.Type);

            foreach (var ordersHistoryRepository in _ordersHistoryRepositories)
            {
                await ordersHistoryRepository.AddAsync(orderHistory);
            }
        }
    }
}
