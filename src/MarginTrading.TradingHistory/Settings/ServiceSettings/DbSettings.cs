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
    }
}
