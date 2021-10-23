// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.MarginTrading.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.CorrelationBroker
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
        public RabbitMqQueueSettings Correlation { get; set; }
    }
}
