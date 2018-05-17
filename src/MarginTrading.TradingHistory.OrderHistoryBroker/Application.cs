using System.Threading.Tasks;
using Common.Log;
using Lykke.SlackNotifications;
using MarginTrading.Contract.BackendContracts;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core.Repositories;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Application : BrokerApplicationBase<OrderFullContract>
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

        protected override async Task HandleMessage(OrderFullContract order)
        {
            var orderHistory = order.ToOrderHistoryDomain();

            foreach (var ordersHistoryRepository in _ordersHistoryRepositories)
            {
                await ordersHistoryRepository.AddAsync(orderHistory);
            }
        }
    }
}
