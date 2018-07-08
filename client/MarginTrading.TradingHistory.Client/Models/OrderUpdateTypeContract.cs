using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderUpdateTypeContract
    {
        Place,
        Activate,
        Change,
        Cancel,
        Reject,
        ExecutionStarted,
        Executed
    }
}
