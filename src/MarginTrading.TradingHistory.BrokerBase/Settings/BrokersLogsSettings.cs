using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.BrokerBase.Settings
{
    public class BrokersLogsSettings
    {
        public StorageMode StorageMode { get; set; }
        
        public string DbConnString { get; set; }
    }
}