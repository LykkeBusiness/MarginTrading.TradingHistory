// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderBlotterSortingColumnContract
    {
        Quantity,
        Price,
        ExchangeRate,
        CreatedOn,
        ModifiedOn,
        Validity
    }
}
