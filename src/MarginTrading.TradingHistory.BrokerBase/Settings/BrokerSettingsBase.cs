using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.BrokerBase.Settings
{
    public class BrokerSettingsBase
    {
        public string MtRabbitMqConnString { get; set; }
        [Optional]
        public bool IsLive { get; set; }
        [Optional]
        public string Env { get; set; }
    }
}