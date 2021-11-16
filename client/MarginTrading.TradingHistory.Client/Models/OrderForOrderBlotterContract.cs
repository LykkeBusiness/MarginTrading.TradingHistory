// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory.Client.Models
{
    public class OrderForOrderBlotterContract
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string CreatedBy { get; set; }
        public string Instrument { get; set; }
        public int Quantity { get; set; }
        public OrderTypeContract OrderType { get; set; }
        public OrderStatusContract OrderStatus { get; set; }
        public decimal? LimitStopPrice { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal? Notional { get; set; }
        public decimal? NotionalEur { get; set; }
        public decimal ExchangeRate { get; set; }
        public OrderDirectionContract Direction { get; set; }
        public OriginatorTypeContract Originator { get; set; }
        public string OrderId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime? Validity { get; set; }
        public string OrderComment { get; set; }
        public decimal? Commission { get; set; }
        public decimal? OnBehalfFee { get; set; }
        public decimal? Spread { get; set; }
        public bool ForcedOpen { get; set; }
    }
}
