// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.TradingHistory.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TradingHistorySettings
    {
        public DbSettings Db { get; set; }
        
        [Optional]
        public bool UseSerilog { get; set; }
    }
}
