using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    [UsedImplicitly]
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        public RabbitMqQueues RabbitMqQueues { get; set; }
        
        public string CancelledTradeIdAttributeName { get; set; }
        
        public string IsCancellationTradeAttributeName { get; set; }
    }
    
    [UsedImplicitly]
    public class Db
    {
        public StorageMode StorageMode { get; set; }
        public string ConnString { get; set; }
    }
    
    [UsedImplicitly]
    public class RabbitMqQueues
    {
        public RabbitMqQueueSettings OrderHistory { get; set; }
    }
}
