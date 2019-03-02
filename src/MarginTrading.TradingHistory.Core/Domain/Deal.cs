using System;
using JetBrains.Annotations;

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
        
        public decimal? OvernightFees { get; }
        public decimal? Commission { get; }
        public decimal? OnBehalfFee { get; }
        public decimal? Taxes { get; }
        [NotNull] public string AdditionalInfo { get; }

        public Deal([NotNull] string dealId, DateTime created, [NotNull] string accountId, [NotNull] string assetPairId,
            [NotNull] string openTradeId, OrderType openOrderType, decimal openOrderVolume, decimal? openOrderExpectedPrice, 
            [NotNull] string closeTradeId, OrderType closeOrderType, decimal closeOrderVolume, decimal? closeOrderExpectedPrice, 
            PositionDirection direction, decimal volume, OriginatorType originator, decimal openPrice, 
            decimal openFxPrice, decimal closePrice, decimal closeFxPrice, decimal fpl, string additionalInfo, decimal pnlOfTheLastDay, 
            decimal? overnightFees = null, decimal? commission = null, decimal? onBehalfFee = null, decimal? taxes = null)
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
            
            OvernightFees = overnightFees;
            Commission = commission;
            OnBehalfFee = onBehalfFee;
            Taxes = taxes;
        }
    }
}
