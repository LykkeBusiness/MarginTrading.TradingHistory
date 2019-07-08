// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Client.Models
{
    public class RelatedOrderInfoContract
    {
        public OrderTypeContract Type { get; set; }
        public string Id { get; set; }
    }
}