﻿using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class BaseOrderContract
    {
        public string Id { get; set; }
        public string Instrument { get; set; }
        public decimal Volume { get; set; }
        public DateTime CreateDate { get; set; }
        public IList<MatchedOrderContract> MatchedOrders { get; set; }
    }
}
