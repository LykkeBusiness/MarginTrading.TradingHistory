using System.Threading.Tasks;
using Common.Log;
using Lykke.SlackNotifications;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.BrokerBase;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.TradeHistoryBroker
{
    public class Application : BrokerApplicationBase<TradeContract>
    {
        private readonly ITradesRepository _tradesRepository;
        private readonly IConvertService _convertService;
        private readonly Settings _settings;

        public Application(ITradesRepository tradesRepository, ILog logger, IConvertService convertService,
            Settings settings, CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender)
            : base(logger, slackNotificationsSender, applicationInfo)
        {
            _tradesRepository = tradesRepository;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.Trades.ExchangeName;
        protected override string QueuePostfix => ".Trades";

        protected override Task HandleMessage(TradeContract tradeContract)
        {
            var trade = tradeContract.ToTradeDomain();
            
            return _tradesRepository.UpsertAsync(trade);
        }
    }
}
