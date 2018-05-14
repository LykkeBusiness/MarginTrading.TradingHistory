using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using MarginTrading.TradingHistory.BrokerBase.Settings;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
    
    public class Db
    {
        [Optional, CanBeNull]
        public string HistoryConnString { get; set; }
        [Optional, CanBeNull] 
        public string ReportsSqlConnString { get; set; }
    }
    
    public class RabbitMqQueues
    {
        public RabbitMqQueueSettings OrderHistory { get; set; }
    }
}
