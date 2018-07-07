﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Core.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PositionHistoryType
    {
        Open,
        PartiallyClose,
        Close
    }
}