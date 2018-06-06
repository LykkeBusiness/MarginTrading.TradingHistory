﻿using System;
using System.Collections.Generic;
using AzureStorage;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.Serializers;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class OrderHistoryEntity : AzureTableEntity, IOrderHistory
    {
        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string Id { get; set; }
        public long Code { get; set; }
        public string ClientId { get; set; }
        public string TradingConditionId { get; set; }
        public string AccountAssetId { get; set; }
        public string Instrument { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        decimal? IOrderHistory.ExpectedOpenPrice => (decimal?) ExpectedOpenPrice;
        public double? ExpectedOpenPrice { get; set; }
        decimal IOrderHistory.OpenPrice => (decimal) OpenPrice;
        public double OpenPrice { get; set; }
        decimal IOrderHistory.ClosePrice => (decimal) ClosePrice;
        public double ClosePrice { get; set; }
        decimal IOrderHistory.Volume => (decimal) Volume;
        public double Volume { get; set; }
        decimal IOrderHistory.MatchedVolume => (decimal) MatchedVolume;
        public double MatchedVolume { get; set; }
        decimal IOrderHistory.MatchedCloseVolume => (decimal) MatchedCloseVolume;
        public double MatchedCloseVolume { get; set; }
        decimal? IOrderHistory.TakeProfit => (decimal?) TakeProfit;
        public double? TakeProfit { get; set; }
        decimal? IOrderHistory.StopLoss => (decimal?) StopLoss;
        public double? StopLoss { get; set; }
        decimal IOrderHistory.Fpl => (decimal) Fpl;
        public double Fpl { get; set; }
        decimal IOrderHistory.PnL => (decimal) PnL;
        public double PnL { get; set; }
        decimal IOrderHistory.InterestRateSwap => (decimal) InterestRateSwap;
        public double InterestRateSwap { get; set; }
        decimal IOrderHistory.CommissionLot => (decimal) CommissionLot;
        public double CommissionLot { get; set; }
        decimal IOrderHistory.OpenCommission => (decimal) OpenCommission;
        public double OpenCommission { get; set; }
        decimal IOrderHistory.CloseCommission => (decimal) CloseCommission;
        public double CloseCommission { get; set; }
        decimal IOrderHistory.QuoteRate => (decimal) QuoteRate;
        public double QuoteRate { get; set; }
        public int AssetAccuracy { get; set; }
        decimal IOrderHistory.MarginInit => (decimal) MarginInit;
        public double MarginInit { get; set; }
        decimal IOrderHistory.MarginMaintenance => (decimal) MarginMaintenance;
        public double MarginMaintenance { get; set; }
        public DateTime? StartClosingDate { get; set; }
        public string Type { get; set; }
        OrderDirection IOrderHistory.Type => Type.ParseEnum(OrderDirection.Buy);
        public string Status { get; set; }
        OrderStatus IOrderHistory.Status => Status.ParseEnum(OrderStatus.Closed);
        public string CloseReason { get; set; }
        OrderCloseReason IOrderHistory.CloseReason => CloseReason.ParseEnum(OrderCloseReason.Close);
        public string FillType { get; set; }
        OrderFillType IOrderHistory.FillType => FillType.ParseEnum(OrderFillType.FillOrKill);
        public string RejectReason { get; set; }
        OrderRejectReason IOrderHistory.RejectReason => RejectReason.ParseEnum(OrderRejectReason.None);
        public string RejectReasonText { get; set; }
        public string Comment { get; set; }
        decimal IOrderHistory.SwapCommission => (decimal) SwapCommission;
        public double SwapCommission { get; set; }
        public string EquivalentAsset { get; set; }
        decimal IOrderHistory.OpenPriceEquivalent => (decimal) OpenPriceEquivalent;
        public double OpenPriceEquivalent { get; set; }
        decimal IOrderHistory.ClosePriceEquivalent => (decimal) ClosePriceEquivalent;
        public double ClosePriceEquivalent { get; set; }
        public string OpenExternalOrderId { get; set; }
        public string OpenExternalProviderId { get; set; }
        public string CloseExternalOrderId { get; set; }
        public string CloseExternalProviderId { get; set; }
        MatchingEngineMode IOrderHistory.MatchingEngineMode =>
            MatchingEngineMode.ParseEnum(Core.Domain.MatchingEngineMode.MarketMaker);
        public string MatchingEngineMode { get; set; }
        public string LegalEntity { get; set; }
        OrderUpdateType IOrderHistory.OrderUpdateType => OrderUpdateType.ParseEnum(Core.Domain.OrderUpdateType.Close);
        public string OrderUpdateType { get; set; }
        //public string ParentPositionId { get; set; }
        //public string ParentOrderId { get; set; }

//        public DateTime UpdateTimestamp
//        {
//            get => DateTime.ParseExact(RowKey, RowKeyDateTimeFormat.Iso.ToDateTimeMask(), null);
//            set => RowKey = value.ToString(RowKeyDateTimeFormat.Iso.ToDateTimeMask());
//        }

        [ValueSerializer(typeof(JsonStorageValueSerializer))]
        public List<MatchedOrder> MatchedOrders { get; set; } = new List<MatchedOrder>();

        [ValueSerializer(typeof(JsonStorageValueSerializer))]
        public List<MatchedOrder> MatchedCloseOrders { get; set; } = new List<MatchedOrder>();

        public static string GeneratePartitionKey(string accountId)
        {
            return accountId;
        }

        public static OrderHistoryEntity Create(IOrderHistory src)
        {
            return new OrderHistoryEntity
            {
                Id = src.Id,
                Code = src.Code,
                AccountId = src.AccountId,
                ClientId = src.ClientId,
                TradingConditionId = src.TradingConditionId,
                AccountAssetId = src.AccountAssetId,
                Instrument = src.Instrument,
                Type = src.Type.ToString(),
                CreateDate = src.CreateDate,
                OpenDate = src.OpenDate,
                CloseDate = src.CloseDate,
                ExpectedOpenPrice = (double?) src.ExpectedOpenPrice,
                OpenPrice = (double) src.OpenPrice,
                ClosePrice = (double) src.ClosePrice,
                TakeProfit = (double?) src.TakeProfit,
                StopLoss = (double?) src.StopLoss,
                Fpl = (double) src.Fpl,
                PnL = (double) src.PnL,
                InterestRateSwap = (double) src.InterestRateSwap,
                CommissionLot = (double) src.CommissionLot,
                OpenCommission = (double) src.OpenCommission,
                CloseCommission = (double) src.CloseCommission,
                QuoteRate = (double) src.QuoteRate,
                AssetAccuracy = src.AssetAccuracy,
                MarginInit = (double) src.MarginInit,
                MarginMaintenance = (double) src.MarginMaintenance,
                StartClosingDate = src.StartClosingDate,
                Status = src.Status.ToString(),
                CloseReason = src.CloseReason.ToString(),
                FillType = src.FillType.ToString(),
                Volume = (double) src.Volume,
                MatchedVolume = (double) src.MatchedVolume,
                MatchedCloseVolume = (double) src.MatchedCloseVolume,
                RejectReason = src.RejectReason.ToString(),
                RejectReasonText = src.RejectReasonText,
                MatchedOrders = src.MatchedOrders,
                MatchedCloseOrders = src.MatchedCloseOrders,
                SwapCommission = (double) src.SwapCommission,
                EquivalentAsset = src.EquivalentAsset,
                OpenPriceEquivalent = (double) src.OpenPriceEquivalent,
                ClosePriceEquivalent = (double) src.ClosePriceEquivalent,
                Comment = src.Comment,
                OrderUpdateType = src.OrderUpdateType.ToString(),
                OpenExternalOrderId = src.OpenExternalOrderId,
                OpenExternalProviderId = src.OpenExternalProviderId,
                CloseExternalOrderId = src.CloseExternalOrderId,
                CloseExternalProviderId = src.CloseExternalProviderId,
                MatchingEngineMode = src.MatchingEngineMode.ToString(),
                LegalEntity = src.LegalEntity,
                //UpdateTimestamp = src.UpdateTimestamp,
                //ParentPositionId = src.ParentPositionId,
                //ParentOrderId = src.ParentOrderId,
            };
        }
    }
}
