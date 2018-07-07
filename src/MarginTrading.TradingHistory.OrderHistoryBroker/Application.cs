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
        private readonly IOrderHistoryRepository[] _orderHistoryRepositories;
        private readonly Settings _settings;

        public Application(IOrderHistoryRepository[] orderHistoryRepositories, ILog logger,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender) : base(logger, slackNotificationsSender,
            applicationInfo)
        {
            _orderHistoryRepositories = orderHistoryRepositories;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.OrderHistory.ExchangeName;

        protected override async Task HandleMessage(OrderHistoryEvent historyEvent)
        {
            var orderHistory = historyEvent.OrderSnapshot.ToOrderHistoryDomain(historyEvent.Type);

            foreach (var ordersHistoryRepository in _orderHistoryRepositories)
            {
                await ordersHistoryRepository.AddAsync(orderHistory);
            }
        }
    }
}
