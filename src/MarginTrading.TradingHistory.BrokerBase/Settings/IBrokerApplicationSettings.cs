using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.BrokerBase.Settings
{
    public interface IBrokerApplicationSettings<TBrokerSettings> 
        where TBrokerSettings : BrokerSettingsBase
    {
        SlackNotificationSettings SlackNotifications { get; }
        
        [Optional, CanBeNull]
        BrokersLogsSettings MtBrokersLogs { get; set; }
        
        BrokerSettingsRoot<TBrokerSettings> MtBackend { get; set; }
    }
}
