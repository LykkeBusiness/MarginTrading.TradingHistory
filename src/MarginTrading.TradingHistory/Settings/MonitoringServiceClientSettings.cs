using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }
}
