using Lykke.AzureQueueIntegration;

namespace MarginTrading.TradingHistory.BrokerBase.Settings
{
    public class SlackNotificationSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
    }
}
