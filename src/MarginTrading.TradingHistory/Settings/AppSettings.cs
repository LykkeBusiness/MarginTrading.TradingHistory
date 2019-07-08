// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using MarginTrading.TradingHistory.Settings.ServiceSettings;
using MarginTrading.TradingHistory.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;
using Lykke.Snow.Common.Startup.ApiKey;

namespace MarginTrading.TradingHistory.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public TradingHistorySettings TradingHistoryService { get; set; }

        [Optional, CanBeNull]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        [Optional]
        public ClientSettings TradingHistoryClient { get; set; } = new ClientSettings();
    }
}
