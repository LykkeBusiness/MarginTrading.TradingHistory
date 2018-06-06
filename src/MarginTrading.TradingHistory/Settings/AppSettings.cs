using JetBrains.Annotations;
using MarginTrading.TradingHistory.Settings.ServiceSettings;
using MarginTrading.TradingHistory.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public TradingHistorySettings TradingHistoryService { get; set; }

        [Optional, CanBeNull]
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
