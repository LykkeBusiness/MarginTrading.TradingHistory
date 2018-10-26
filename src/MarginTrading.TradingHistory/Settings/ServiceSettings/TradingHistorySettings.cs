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
