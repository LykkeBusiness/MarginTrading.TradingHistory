﻿using System;
using System.Collections.Generic;
using Common;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class OrderHistoryEntity : IOrderHistory
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public string ParentOrderId { get; set; }
        public string PositionId { get; set; }
        OrderDirection IOrderHistory.Direction => Direction.ParseEnum<OrderDirection>();
        public string Direction { get; set; }
        OrderType IOrderHistory.Type => Type.ParseEnum<OrderType>();
        public string Type { get; set; }
        OrderStatus IOrderHistory.Status => Status.ParseEnum<OrderStatus>();
        public string Status { get; set; }
        OriginatorType IOrderHistory.Originator => Originator.ParseEnum<OriginatorType>();
        public string Originator { get; set; }
        OriginatorType? IOrderHistory.CancellationOriginator => Originator?.ParseEnum<OriginatorType>();
        public string CancellationOriginator { get; set; }
        public decimal Volume { get; set; }
        public decimal? ExpectedOpenPrice { get; set; }
        public decimal? ExecutionPrice { get; set; }
        public decimal FxRate { get; set; }
        public bool ForceOpen { get; set; }
        public DateTime? ValidityTime { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime ModifiedTimestamp { get; set; }
        public long Code { get; set; }
        public DateTime? ActivatedTimestamp { get; set; }
        public DateTime? ExecutionStartedTimestamp { get; set; }
        public DateTime? ExecutedTimestamp { get; set; }
        public DateTime? CanceledTimestamp { get; set; }
        public DateTime? Rejected { get; set; }
        public string TradingConditionId { get; set; }
        public string AccountAssetId { get; set; }
        public string EquivalentAsset { get; set; }
        public decimal EquivalentRate { get; set; }
        OrderRejectReason IOrderHistory.RejectReason => RejectReason.ParseEnum<OrderRejectReason>();
        public string RejectReason { get; set; }
        public string RejectReasonText { get; set; }
        public string Comment { get; set; }
        public string ExternalOrderId { get; set; }
        public string ExternalProviderId { get; set; }
        public string MatchingEngineId { get; set; }
        public string LegalEntity { get; set; }

        List<MatchedOrder> IOrderHistory.MatchedOrders => string.IsNullOrEmpty(MatchedOrders)
            ? new List<MatchedOrder>()
            : MatchedOrders.DeserializeJson<List<MatchedOrder>>();

        List<RelatedOrderInfo> IOrderHistory.RelatedOrderInfos => string.IsNullOrEmpty(RelatedOrderInfos)
            ? new List<RelatedOrderInfo>()
            : RelatedOrderInfos.DeserializeJson<List<RelatedOrderInfo>>();

        OrderUpdateType IOrderHistory.UpdateType => UpdateType.ParseEnum<OrderUpdateType>();
        public string UpdateType { get; set; }
        public string AdditionalInfo { get; set; }

        public string MatchedOrders { get; set; }
        public string RelatedOrderInfos { get; set; }
        
        public static OrderHistoryEntity Create(IOrderHistory order)
        {
            return new OrderHistoryEntity
            {
                Id = order.Id,
                AccountId = order.AccountId,
                AssetPairId = order.AssetPairId,
                CreatedTimestamp = order.CreatedTimestamp,
                Direction = order.Direction.ToString(),
                ExecutionPrice = order.ExecutionPrice,
                FxRate = order.FxRate,
                ExpectedOpenPrice = order.ExpectedOpenPrice,
                ForceOpen = order.ForceOpen,
                ModifiedTimestamp = order.ModifiedTimestamp,
                Originator = order.Originator.ToString(),
                CancellationOriginator = order.CancellationOriginator?.ToString(),
                ParentOrderId = order.ParentOrderId,
                PositionId = order.PositionId,
                Status = order.Status.ToString(),
                Type = order.Type.ToString(),
                ValidityTime = order.ValidityTime,
                Volume = order.Volume,
                //------
                AccountAssetId = order.AccountAssetId,
                EquivalentAsset = order.EquivalentAsset,
                ActivatedTimestamp = order.ActivatedTimestamp,
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
                RejectReason = order.RejectReason.ToString(),
                RejectReasonText = order.RejectReasonText,
                RelatedOrderInfos = order.RelatedOrderInfos.ToJson(),
                TradingConditionId = order.TradingConditionId,
                MatchedOrders = order.MatchedOrders.ToJson(),
                UpdateType = order.UpdateType.ToString(),
                AdditionalInfo = order.AdditionalInfo
            };
        }
    }
}
