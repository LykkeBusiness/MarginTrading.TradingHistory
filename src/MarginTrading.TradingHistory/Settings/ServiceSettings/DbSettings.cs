// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.SettingsReader.Attributes;
using MarginTrading.TradingHistory.Core;

namespace MarginTrading.TradingHistory.Settings.ServiceSettings
{
    public class DbSettings
    {
        public StorageMode StorageMode { get; set; }
        
        [Optional]
        public string LogsConnString { get; set; }
        
        //[AzureTableCheck]
        public string HistoryConnString { get; set; }

        [Optional]
        public TimeSpan OrderHistoryForSupportExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}
