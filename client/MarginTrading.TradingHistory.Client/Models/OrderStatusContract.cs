using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatusContract
    {
        Active = 1,
        Inactive = 2,
        Executed = 3,
        Canceled = 4,
        Rejected = 5,
        Expired = 6
    }
}
