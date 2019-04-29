using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PositionHistoryTypeContract
    {
        Open,
        PartiallyClose,
        Close
    }
}
