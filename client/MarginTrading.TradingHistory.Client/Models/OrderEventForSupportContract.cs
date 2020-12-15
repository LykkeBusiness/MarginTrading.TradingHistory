using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderEventForSupportContract
    {
        public string OrderId { get; set; }

        public string ClientId { get; set; }

        public DateTime ExecutedTimestamp { get; set; }

        public DateTime CreatedTimestamp { get; set; }

        public string Status { get; set; }

        public decimal Volume { get; set; }

        public decimal ExecutionPrice { get; set; }

        public string AssetPairId { get; set; }
    }

    public class OrderEventsForSupportRequest
    {
        public string Id { get; set; }

        public DateTime? ExecutedTimestampFrom { get; set; }

        public DateTime? ExecutedTimestampTo { get; set; }

        public DateTime? CreatedTimestampFrom { get; set; }

        public DateTime? CreatedTimestampTo { get; set; }

        public IEnumerable<string> AssetPairIds { get; set; }

        public string ClientId { get; set; }

        public string ExecutionPrice { get; set; }

        public int Skip { get; set; } = 0;

        public int Take { get; set; } = 20;
    }
}
