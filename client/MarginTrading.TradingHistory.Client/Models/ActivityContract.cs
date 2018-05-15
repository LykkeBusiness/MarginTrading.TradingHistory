using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class ActivityContract
    {
        public  string Id { get; set; }
        public string InstrumentId { get; set; }
        public string Category { get; set; }
        public ActivityTypeContract Type { get; set; }
        public string Comment { get; set; }
        public List<string> ReferenceIds { get; set; }
    }
}
