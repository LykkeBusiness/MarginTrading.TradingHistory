using System;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class DealInfo
    {
        public string PositionId { get; set; }
        public DateTime Created { get; set; }
        public string OpenTradeId { get; set; }
        public string CloseTradeId { get; set; }
        public decimal Volume { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal Fpl { get; set; }
    }
}