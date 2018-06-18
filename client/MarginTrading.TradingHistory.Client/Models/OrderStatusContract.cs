using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatusContract
    {
        Placed = 0,
        Inactive = 1,
        Active = 2,
        ExecutionStarted = 3,
        Executed = 4,
        Canceled = 5,
        Rejected = 6,
        Expired = 7
    }
}
