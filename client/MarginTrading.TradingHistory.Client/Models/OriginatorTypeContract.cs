using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OriginatorTypeContract
    {
        Investor = 1,
        System = 2,
        OnBehalf = 3
    }
}
