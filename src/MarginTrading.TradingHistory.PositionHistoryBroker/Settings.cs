﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using MarginTrading.TradingHistory.BrokerBase.Settings;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
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
        public RabbitMqQueueSettings PositionsHistory { get; set; }
    }
}
