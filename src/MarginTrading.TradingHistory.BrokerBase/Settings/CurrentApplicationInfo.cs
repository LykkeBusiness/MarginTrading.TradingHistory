using System;

namespace MarginTrading.TradingHistory.BrokerBase.Settings
{
    public class CurrentApplicationInfo
    {
        public CurrentApplicationInfo(string applicationVersion, string applicationName)
        {
            ApplicationVersion = applicationVersion;
            ApplicationName = applicationName;
            EnvInfo = Environment.GetEnvironmentVariable("ENV_INFO");
        }

        public string ApplicationVersion { get; }
        public string ApplicationName { get; }
        public string EnvInfo { get; }

        public string ApplicationFullName => $"{ApplicationName}:{ApplicationVersion}";
    }
}