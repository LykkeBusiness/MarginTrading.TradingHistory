// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PositionDirection
    {
        /// <summary>
        /// Position is profitable if the price goes up 
        /// </summary>
        Long = 1,
        
        /// <summary>
        /// Position is profitable if the price goes down
        /// </summary>
        Short = 2
    }
}