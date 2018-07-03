﻿using System;
using System.Collections.Generic;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IPositionHistory
    {
        string Id { get; }
        string DealId { get; }
        long Code { get; }
        string AssetPairId { get; }
        PositionDirection Direction { get; }
        decimal Volume { get; }
        string AccountId { get; }
        string TradingConditionId { get; }
        string AccountAssetId { get; }
        decimal? ExpectedOpenPrice { get; }
        string OpenMatchingEngineId { get; }
        DateTime OpenDate { get; }
        string OpenTradeId { get; }
        decimal OpenPrice { get; }
        decimal OpenFxPrice { get; }
        string EquivalentAsset { get; }
        decimal OpenPriceEquivalent { get; }
        List<RelatedOrderInfo> RelatedOrders { get; }
        string LegalEntity { get; }
        OriginatorType OpenOriginator { get; }
        string ExternalProviderId { get; }
        decimal SwapCommissionRate { get; }
        decimal OpenCommissionRate { get; }
        decimal CloseCommissionRate { get; }
        decimal CommissionLot { get; }
        string CloseMatchingEngineId { get; }
        decimal ClosePrice { get; }
        decimal CloseFxPrice { get; }
        decimal ClosePriceEquivalent { get; }
        DateTime? StartClosingDate { get; }
        DateTime? CloseDate { get; }
        OriginatorType? CloseOriginator { get; }
        OrderCloseReason CloseReason { get; }
        string CloseComment { get; }
        List<string> CloseTrades { get; }
        DateTime? LastModified { get; }
        decimal TotalPnL { get; }
        PositionHistoryType HistoryType { get; }
        DealInfo DealInfo { get; }
        DateTime HistoryTimestamp { get; }
    }
}