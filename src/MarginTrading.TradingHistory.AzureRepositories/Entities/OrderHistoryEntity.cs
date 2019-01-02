using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.Serializers;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class OrderHistoryEntity : AzureTableEntity, IOrderHistory
    {
        public string Id { get; set; }

        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string AssetPairId { get; set; }
        public string ParentOrderId { get; set; }
        public string PositionId { get; set; }
        public OrderDirection Direction { get; set; }
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public OriginatorType Originator { get; set; }
        public OriginatorType? CancellationOriginator { get; set; }
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
        public OrderRejectReason RejectReason { get; set; }
        public string RejectReasonText { get; set; }
        public string Comment { get; set; }
        public string ExternalOrderId { get; set; }
        public string ExternalProviderId { get; set; }
        public string MatchingEngineId { get; set; }
        public string LegalEntity { get; set; }

        [ValueSerializer(typeof(JsonStorageValueSerializer))]
        public List<MatchedOrder> MatchedOrders { get; set; } = new List<MatchedOrder>();

        [ValueSerializer(typeof(JsonStorageValueSerializer))]
        public List<RelatedOrderInfo> RelatedOrderInfos { get; set; }

        public OrderUpdateType UpdateType { get; set; }
        public string AdditionalInfo { get; set; }
        public string CorrelationId { get; set; }
        public string CancelledBy { get; set; }

        public static OrderHistoryEntity Create(IOrderHistory order)
        {
            return new OrderHistoryEntity
            {
                Id = order.Id,
                AccountId = order.AccountId,
                AssetPairId = order.AssetPairId,
                CreatedTimestamp = order.CreatedTimestamp,
                Direction = order.Direction,
                ExecutionPrice = order.ExecutionPrice,
                FxRate = order.FxRate,
                ExpectedOpenPrice = order.ExpectedOpenPrice,
                ForceOpen = order.ForceOpen,
                ModifiedTimestamp = order.ModifiedTimestamp,
                Originator = order.Originator,
                CancellationOriginator = order.CancellationOriginator,
                ParentOrderId = order.ParentOrderId,
                PositionId = order.PositionId,
                Status = order.Status,
                Type = order.Type,
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
                RejectReason = order.RejectReason,
                RejectReasonText = order.RejectReasonText,
                RelatedOrderInfos = order.RelatedOrderInfos,
                TradingConditionId = order.TradingConditionId,
                MatchedOrders = order.MatchedOrders,
                UpdateType = order.UpdateType,
                AdditionalInfo = order.AdditionalInfo,
                CorrelationId = order.CorrelationId,
                CancelledBy = order.CancelledBy
            };
        }
    }
}
