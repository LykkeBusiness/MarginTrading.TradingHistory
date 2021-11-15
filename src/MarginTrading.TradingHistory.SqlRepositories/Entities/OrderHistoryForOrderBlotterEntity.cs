// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class OrderHistoryForOrderBlotterEntity: IOrderHistoryForOrderBlotterWithAdditionalData
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string CreatedBy { get; set; }
        public string AssetPairId { get; set; }
        public int Volume { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public OrderDirection Direction { get; set; }
        public OriginatorType Originator { get; set; }
        public decimal? ExecutionPrice { get; set; }
        public decimal FxRate { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime ModifiedTimestamp { get; set; }
        public DateTime? ValidityTime { get; set; }
        public string Comment { get; set; }
        public bool ForceOpen { get; set; }
        public decimal? ExpectedOpenPrice { get; set; }
        public string ExternalOrderId { get; set; }

        public string AssetName { get; set; }
        public decimal? TakeProfitPrice { get; set; }
        public decimal? StopLossPrice { get; set; }
        public decimal? Commission { get; set; }
        public decimal? OnBehalfFee { get; set; }
        public decimal? Spread { get; set; }
    }
}
