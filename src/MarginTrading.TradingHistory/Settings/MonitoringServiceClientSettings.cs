// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
