using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OriginatorType
    {
        Investor = 1,
        System = 2,
        OnBehalf = 3
    }
}