using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
