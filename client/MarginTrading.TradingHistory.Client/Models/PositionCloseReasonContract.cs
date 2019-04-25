using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PositionCloseReasonContract
    {
        None,
        Close,
        StopLoss,
        TakeProfit,
        StopOut,
    }
}
