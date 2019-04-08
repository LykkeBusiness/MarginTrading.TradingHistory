using System;
using System.Collections.Generic;
using System.Linq;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public static class MappingExtension
    {
        public static OrderHistory ToOrderHistoryDomain(this OrderContract order, OrderHistoryTypeContract historyType)
        {
            var orderContract = new OrderHistory
            {
                Id = order.Id,
                AccountId = order.AccountId,
                AssetPairId = order.AssetPairId,
                CreatedTimestamp = order.CreatedTimestamp,
                Direction = order.Direction.ToType<OrderDirection>(),
                ExecutionPrice = order.ExecutionPrice,
                FxRate = order.FxRate,
                FxAssetPairId = order.FxAssetPairId,
                FxToAssetPairDirection = order.FxToAssetPairDirection.ToType<FxToAssetPairDirection>(),
                ExpectedOpenPrice = order.ExpectedOpenPrice,
                ForceOpen = order.ForceOpen,
                ModifiedTimestamp = order.ModifiedTimestamp,
                Originator = order.Originator.ToType<OriginatorType>(),
                ParentOrderId = order.ParentOrderId,
                PositionId = order.PositionId,
                Status = order.Status.ToType<OrderStatus>(),
                FillType = order.FillType.ToType<OrderFillType>(),
                Type = order.Type.ToType<OrderType>(),
                ValidityTime = order.ValidityTime,
                Volume = order.Volume ?? 0,
                //------
                AccountAssetId = order.AccountAssetId,
                EquivalentAsset = order.EquivalentAsset,
                ActivatedTimestamp = order.ActivatedTimestamp == default(DateTime)
                    ? null
                    : order.ActivatedTimestamp,
                CanceledTimestamp = order.CanceledTimestamp,
                Code = order.Code,
                Comment = order.Comment,
                EquivalentRate = order.EquivalentRate,
                ExecutedTimestamp = order.ExecutedTimestamp,
                ExecutionStartedTimestamp = order.ExecutionStartedTimestamp,
                ExternalOrderId = order.ExternalOrderId,
                ExternalProviderId = order.ExternalProviderId,
                LegalEntity = order.LegalEntity,
                MatchingEngineId = order.MatchingEngineId,
                Rejected = order.Rejected,
                RejectReason = order.RejectReason.ToType<OrderRejectReason>(),
                RejectReasonText = order.RejectReasonText,
                RelatedOrderInfos = order.RelatedOrderInfos.Select(o =>
                    new RelatedOrderInfo {Id = o.Id, Type = o.Type.ToType<OrderType>()}).ToList(),
                TradingConditionId = order.TradingConditionId,
                UpdateType = historyType.ToType<OrderUpdateType>(),
                MatchedOrders = new List<MatchedOrder>(),
                AdditionalInfo = order.AdditionalInfo,
                CorrelationId = order.CorrelationId,
                PendingOrderRetriesCount = order.PendingOrderRetriesCount,
            };

            foreach (var mo in order.MatchedOrders)
            {
                orderContract.MatchedOrders.Add(mo.ToDomain());
            }

            return orderContract;
        }

        private static MatchedOrder ToDomain(this MatchedOrderContract src)
        {
            return new MatchedOrder
            {
                OrderId = src.OrderId,
                MarketMakerId = src.MarketMakerId,
                LimitOrderLeftToMatch = src.LimitOrderLeftToMatch,
                Volume = src.Volume,
                Price = src.Price,
                MatchedDate = src.MatchedDate,
                IsExternal = src.IsExternal
            };
        }
    }
}
