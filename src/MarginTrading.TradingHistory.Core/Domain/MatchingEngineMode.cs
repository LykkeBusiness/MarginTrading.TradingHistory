using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchingEngineMode
    {
        /// <summary>
        /// Market making mode with actual matching on our side
        /// </summary>
        MarketMaker = 1,
        
        /// <summary>
        /// Straight through processing with orders matching on external exchanges
        /// </summary>
        Stp = 2,
    }
}
