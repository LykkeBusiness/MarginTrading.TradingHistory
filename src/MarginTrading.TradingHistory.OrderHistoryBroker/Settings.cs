using Lykke.MarginTrading.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
    
    public class Db
    {
        public StorageMode StorageMode { get; set; }
        public string ConnString { get; set; }
    }
    
    public class RabbitMqQueues
    {
        public RabbitMqQueueSettings OrderHistory { get; set; }
    }
}
