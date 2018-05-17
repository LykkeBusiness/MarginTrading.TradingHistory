using JetBrains.Annotations;

namespace MarginTrading.TradingHistory.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TradingHistorySettings
    {
        public DbSettings Db { get; set; }
    }
}
