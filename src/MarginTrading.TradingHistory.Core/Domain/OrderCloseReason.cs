using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderCloseReason
    {
        
        None,
        Close,
        StopLoss,
        TakeProfit,
        StopOut,
        Canceled,
        CanceledBySystem,
        CanceledByBroker,
        ClosedByBroker,
    }
}
