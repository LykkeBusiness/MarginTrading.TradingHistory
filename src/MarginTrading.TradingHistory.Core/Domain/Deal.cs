// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.TradingHistory.Core.Domain
{
    public class Deal : IDeal
    {
        [NotNull] public string DealId { get; }
        public DateTime Created { get; }
        [NotNull] public string AccountId { get; }
        [NotNull] public string AssetPairId { get; }
        public string OpenTradeId { get; }
        public OrderType OpenOrderType { get; }
        public decimal OpenOrderVolume { get; }
        public decimal? OpenOrderExpectedPrice { get; }
        public string CloseTradeId { get; }
        public OrderType CloseOrderType { get; }
        public decimal CloseOrderVolume { get; }
        public decimal? CloseOrderExpectedPrice { get; }
        public PositionDirection Direction { get; }
        public decimal Volume { get; }
        public OriginatorType Originator { get; }
        public decimal OpenPrice { get; }
        public decimal OpenFxPrice { get; }
        public decimal ClosePrice { get; }
        public decimal CloseFxPrice { get; }
        public decimal Fpl { get; }
        public decimal PnlOfTheLastDay { get; }
        
        [NotNull] public string AdditionalInfo { get; }
        public string CorrelationId { get; }

        public Deal([NotNull] string dealId, DateTime created, [NotNull] string accountId, [NotNull] string assetPairId,
            [NotNull] string openTradeId, OrderType openOrderType, decimal openOrderVolume, decimal? openOrderExpectedPrice, 
            [NotNull] string closeTradeId, OrderType closeOrderType, decimal closeOrderVolume, decimal? closeOrderExpectedPrice, 
            PositionDirection direction, decimal volume, OriginatorType originator, decimal openPrice, decimal openFxPrice,
            decimal closePrice, decimal closeFxPrice, decimal fpl, string additionalInfo, decimal pnlOfTheLastDay, string correlationId)
        {
            DealId = dealId ?? throw new ArgumentNullException(nameof(dealId));
            Created = created;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            OpenTradeId = openTradeId ?? throw new ArgumentNullException(nameof(openTradeId));
            OpenOrderType = openOrderType;
            OpenOrderVolume = openOrderVolume;
            OpenOrderExpectedPrice = openOrderExpectedPrice;
            CloseTradeId = closeTradeId ?? throw new ArgumentNullException(nameof(closeTradeId));
            CloseOrderType = closeOrderType;
            CloseOrderVolume = closeOrderVolume;
            CloseOrderExpectedPrice = closeOrderExpectedPrice;
            Direction = direction;
            Volume = volume;
            Originator = originator;
            OpenPrice = openPrice;
            OpenFxPrice = openFxPrice;
            ClosePrice = closePrice;
            CloseFxPrice = closeFxPrice;
            Fpl = fpl;
            AdditionalInfo = additionalInfo;
            PnlOfTheLastDay = pnlOfTheLastDay;
            CorrelationId = correlationId;
        }

        public static Deal FromPositionHistoryEvent(PositionHistoryEvent evt, string correlationId)
        {
            return new Deal(
                dealId: evt.Deal.DealId,
                created: evt.Deal.Created,
                accountId: evt.PositionSnapshot.AccountId,
                assetPairId: evt.PositionSnapshot.AssetPairId,
                openTradeId: evt.Deal.OpenTradeId,
                openOrderType: evt.Deal.OpenOrderType.ToType<OrderType>(),
                openOrderVolume: evt.Deal.OpenOrderVolume,
                openOrderExpectedPrice: evt.Deal.OpenOrderExpectedPrice,
                closeTradeId: evt.Deal.CloseTradeId,
                closeOrderType: evt.Deal.CloseOrderType.ToType<OrderType>(),
                closeOrderVolume: evt.Deal.CloseOrderVolume,
                closeOrderExpectedPrice: evt.Deal.CloseOrderExpectedPrice,
                direction: evt.PositionSnapshot.Direction.ToType<PositionDirection>(),
                volume: evt.Deal.Volume,
                originator: evt.Deal.Originator.ToType<OriginatorType>(),
                openPrice: evt.Deal.OpenPrice,
                openFxPrice: evt.Deal.OpenFxPrice,
                closePrice: evt.Deal.ClosePrice,
                closeFxPrice: evt.Deal.CloseFxPrice,
                fpl: evt.Deal.Fpl,
                additionalInfo: evt.Deal.AdditionalInfo,
                pnlOfTheLastDay: evt.Deal.PnlOfTheLastDay,
                correlationId: correlationId
            );
        }
    }
}
