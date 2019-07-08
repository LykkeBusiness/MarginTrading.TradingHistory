// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Settings.SlackNotifications
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }
}
