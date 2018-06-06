using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PositionDirection
    {
        Long = 1,
        Short = 2
    }
}
