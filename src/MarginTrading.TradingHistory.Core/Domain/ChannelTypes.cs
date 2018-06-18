using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public static class ChannelTypes
    {
        public const string MarginTrading = "MarginTrading";
        public const string Monitor = "Monitor";
        
    }
}
