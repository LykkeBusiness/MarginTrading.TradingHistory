// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    internal static class MappingExtensions
    {
        public static PositionHistory ToDomain(this PositionHistoryEvent positionHistoryEvent)
        {
            return new PositionHistory
            {
                Id = positionHistoryEvent.PositionSnapshot.Id,
                DealId = positionHistoryEvent.Deal?.DealId,
                Code = positionHistoryEvent.PositionSnapshot.Code,
                AssetPairId = positionHistoryEvent.PositionSnapshot.AssetPairId,
                Direction = positionHistoryEvent.PositionSnapshot.Direction.ToType<PositionDirection>(),
                Volume = positionHistoryEvent.PositionSnapshot.Volume,
                AccountId = positionHistoryEvent.PositionSnapshot.AccountId,
                TradingConditionId = positionHistoryEvent.PositionSnapshot.TradingConditionId,
                AccountAssetId = positionHistoryEvent.PositionSnapshot.AccountAssetId,
                ExpectedOpenPrice = positionHistoryEvent.PositionSnapshot.ExpectedOpenPrice,
                OpenMatchingEngineId = positionHistoryEvent.PositionSnapshot.OpenMatchingEngineId,
                OpenDate = positionHistoryEvent.PositionSnapshot.OpenDate,
                OpenTradeId = positionHistoryEvent.PositionSnapshot.OpenTradeId,
                OpenOrderType = positionHistoryEvent.PositionSnapshot.OpenOrderType.ToType<OrderType>(),
                OpenOrderVolume = positionHistoryEvent.PositionSnapshot.OpenOrderVolume,
                OpenPrice = positionHistoryEvent.PositionSnapshot.OpenPrice,
                OpenFxPrice = positionHistoryEvent.PositionSnapshot.OpenFxPrice,
                EquivalentAsset = positionHistoryEvent.PositionSnapshot.EquivalentAsset,
                OpenPriceEquivalent = positionHistoryEvent.PositionSnapshot.OpenPriceEquivalent,
                RelatedOrders = positionHistoryEvent.PositionSnapshot.RelatedOrders.Select(ro => new RelatedOrderInfo
                {
                    Type = ro.Type.ToType<OrderType>(),
                    Id = ro.Id,
                }).ToList(),
                LegalEntity = positionHistoryEvent.PositionSnapshot.LegalEntity,
                OpenOriginator = positionHistoryEvent.PositionSnapshot.OpenOriginator.ToType<OriginatorType>(),
                ExternalProviderId = positionHistoryEvent.PositionSnapshot.ExternalProviderId,
                SwapCommissionRate = positionHistoryEvent.PositionSnapshot.SwapCommissionRate,
                OpenCommissionRate = positionHistoryEvent.PositionSnapshot.OpenCommissionRate,
                CloseCommissionRate = positionHistoryEvent.PositionSnapshot.CloseCommissionRate,
                CommissionLot = positionHistoryEvent.PositionSnapshot.CommissionLot,
                CloseMatchingEngineId = positionHistoryEvent.PositionSnapshot.CloseMatchingEngineId,
                ClosePrice = positionHistoryEvent.PositionSnapshot.ClosePrice,
                CloseFxPrice = positionHistoryEvent.PositionSnapshot.CloseFxPrice,
                ClosePriceEquivalent = positionHistoryEvent.PositionSnapshot.ClosePriceEquivalent,
                StartClosingDate = positionHistoryEvent.PositionSnapshot.StartClosingDate,
                CloseDate = positionHistoryEvent.PositionSnapshot.CloseDate,
                CloseOriginator = positionHistoryEvent.PositionSnapshot.CloseOriginator?.ToType<OriginatorType>(),
                CloseReason = positionHistoryEvent.PositionSnapshot.CloseReason.ToType<OrderCloseReason>(),
                CloseComment = positionHistoryEvent.PositionSnapshot.CloseComment,
                CloseTrades = positionHistoryEvent.PositionSnapshot.CloseTrades,
                FxAssetPairId = positionHistoryEvent.PositionSnapshot.FxAssetPairId,
                FxToAssetPairDirection = positionHistoryEvent.PositionSnapshot.FxToAssetPairDirection.ToType<FxToAssetPairDirection>(),
                LastModified = positionHistoryEvent.PositionSnapshot.LastModified,
                TotalPnL = positionHistoryEvent.PositionSnapshot.TotalPnL,
                ChargedPnl = positionHistoryEvent.PositionSnapshot.ChargedPnl,
                AdditionalInfo = positionHistoryEvent.PositionSnapshot.AdditionalInfo,
                HistoryType = positionHistoryEvent.EventType.ToType<PositionHistoryType>(),
                HistoryTimestamp = positionHistoryEvent.Timestamp,
                ForceOpen = positionHistoryEvent.PositionSnapshot.ForceOpen,
            };
        }
        
        public static PositionHistory AddCorrelationId(this PositionHistory positionHistory, string correlationId)
        {
            positionHistory.CorrelationId = correlationId;
            return positionHistory;
        }
    }
}
