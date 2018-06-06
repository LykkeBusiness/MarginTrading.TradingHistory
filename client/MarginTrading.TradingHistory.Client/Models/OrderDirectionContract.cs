using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    /// <summary>
    /// The direction of an order
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderDirectionContract
    {
        /// <summary>
        /// Order to buy the quoting asset of a pair
        /// </summary>
        Buy = 1,
        
        /// <summary>
        /// Order to sell the quoting asset of a pair
        /// </summary>
        Sell = 2
    }
}
