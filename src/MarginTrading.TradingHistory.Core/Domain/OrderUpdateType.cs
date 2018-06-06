using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderUpdateType
    {
        Place,
        Cancel,
        Activate,
        Reject,
        Closing,
        Close,
        ChangeOrderLimits,
    }
}
