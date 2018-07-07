using System;
using System.Collections.Generic;
using Common;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class PositionsHistoryEntity : IPositionHistory
    {
        public string Id { get; set; }
        public string DealId { get; set; }
        public long Code { get; set; }
        public string AssetPairId { get; set; }
        PositionDirection IPositionHistory.Direction => Direction.ParseEnum<PositionDirection>();
        public string Direction { get; set; }
        public decimal Volume { get; set; }
        public string AccountId { get; set; }
        public string TradingConditionId { get; set; }
        public string AccountAssetId { get; set; }
        public decimal? ExpectedOpenPrice { get; set; }
        public string OpenMatchingEngineId { get; set; }
        public DateTime OpenDate { get; set; }
        public string OpenTradeId { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public string EquivalentAsset { get; set; }
        public decimal OpenPriceEquivalent { get; set; }
        public string LegalEntity { get; set; }
        OriginatorType IPositionHistory.OpenOriginator => OpenOriginator.ParseEnum<OriginatorType>();
        public string OpenOriginator { get; set; }
        public string ExternalProviderId { get; set; }
        public decimal SwapCommissionRate { get; set; }
        public decimal OpenCommissionRate { get; set; }
        public decimal CloseCommissionRate { get; set; }
        public decimal CommissionLot { get; set; }
        public string CloseMatchingEngineId { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal ClosePriceEquivalent { get; set; }
        public DateTime? StartClosingDate { get; set; }
        public DateTime? CloseDate { get; set; }
        OriginatorType? IPositionHistory.CloseOriginator => string.IsNullOrEmpty(CloseOriginator)
            ? (OriginatorType?) null
            : CloseOriginator.ParseEnum<OriginatorType>();
        public string CloseOriginator { get; set; }
        OrderCloseReason IPositionHistory.CloseReason => CloseReason.ParseEnum<OrderCloseReason>();
        public string CloseReason { get; set; }
        public string CloseComment { get; set; }
        public DateTime? LastModified { get; set; }
        public decimal TotalPnL { get; set; }
        public decimal ChargedPnl { get; set; }
        PositionHistoryType IPositionHistory.HistoryType => HistoryType.ParseEnum<PositionHistoryType>();
        public string HistoryType { get; set; }

        public DateTime HistoryTimestamp { get; set; }

        List<RelatedOrderInfo> IPositionHistory.RelatedOrders => string.IsNullOrEmpty(RelatedOrders)
            ? new List<RelatedOrderInfo>()
            : RelatedOrders.DeserializeJson<List<RelatedOrderInfo>>();
        
        List<string> IPositionHistory.CloseTrades => string.IsNullOrEmpty(CloseTrades)
            ? new List<string>()
            : CloseTrades.DeserializeJson<List<string>>();
        
        public string RelatedOrders { get; set; }
        public string CloseTrades { get; set; }

        public static PositionsHistoryEntity Create(IPositionHistory history)
        {
            return new PositionsHistoryEntity
            {
                DealId = history.DealId,
                AccountAssetId = history.AccountAssetId,
                AccountId = history.AccountId,
                AssetPairId = history.AssetPairId,
                CloseComment = history.CloseComment,
                CloseCommissionRate = history.CloseCommissionRate,
                CloseDate = history.CloseDate,
                CloseFxPrice = history.CloseFxPrice,
                CloseMatchingEngineId = history.CloseMatchingEngineId,
                CloseOriginator = history.CloseOriginator?.ToString(),
                ClosePrice = history.ClosePrice,
                ClosePriceEquivalent = history.ClosePriceEquivalent,
                CloseReason = history.CloseReason.ToString(),
                CloseTrades = history.CloseTrades.ToJson(),
                Code = history.Code,
                CommissionLot = history.CommissionLot,
                Direction = history.Direction.ToString(),
                EquivalentAsset = history.EquivalentAsset,
                ExpectedOpenPrice = history.ExpectedOpenPrice,
                ExternalProviderId = history.ExternalProviderId,
                HistoryType = history.HistoryType.ToString(),
                Id = history.Id,
                LastModified = history.LastModified,
                LegalEntity = history.LegalEntity,
                OpenCommissionRate = history.OpenCommissionRate,
                OpenDate = history.OpenDate,
                OpenFxPrice = history.OpenFxPrice,
                OpenMatchingEngineId = history.OpenMatchingEngineId,
                OpenOriginator = history.OpenOriginator.ToString(),
                OpenPrice = history.OpenPrice,
                OpenPriceEquivalent = history.OpenPriceEquivalent,
                OpenTradeId = history.OpenTradeId,
                RelatedOrders = history.RelatedOrders.ToJson(),
                StartClosingDate = history.CloseDate,
                SwapCommissionRate = history.SwapCommissionRate,
                TotalPnL = history.TotalPnL,
                TradingConditionId = history.TradingConditionId,
                Volume = history.Volume,
                HistoryTimestamp = history.HistoryTimestamp
            };
        }
    }
}
