// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class DealEntity : IDeal
    {
        public string DealId { get; set; }
        public DateTime Created { get; set; }
        public string AccountId { get; set; }
        public string AssetPairId { get; set; }
        public string OpenTradeId { get; set; }
        OrderType IDeal.OpenOrderType => Enum.Parse<OrderType>(OpenOrderType);
        public string OpenOrderType { get; set; }
        public decimal OpenOrderVolume { get; set; }
        public decimal? OpenOrderExpectedPrice { get; set; }
        public string CloseTradeId { get; set; }
        OrderType IDeal.CloseOrderType => Enum.Parse<OrderType>(CloseOrderType);
        public string CloseOrderType { get; set; }
        public decimal CloseOrderVolume { get; set; }
        public decimal? CloseOrderExpectedPrice { get; set; }
        PositionDirection IDeal.Direction => Enum.Parse<PositionDirection>(Direction); 
        public string Direction { get; set; }
        public decimal Volume { get; set; }
        OriginatorType IDeal.Originator => Enum.Parse<OriginatorType>(Originator);
        public string Originator { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal Fpl { get; set; }
        public string AdditionalInfo { get; set; }
        public decimal PnlOfTheLastDay { get; set; }

        public static DealEntity Create(IDeal deal)
        {
            return new DealEntity
            {
                DealId = deal.DealId,
                Created = deal.Created,
                AccountId = deal.AccountId,
                AssetPairId = deal.AssetPairId,
                OpenTradeId = deal.OpenTradeId,
                OpenOrderType = deal.OpenOrderType.ToString(),
                OpenOrderVolume = deal.OpenOrderVolume,
                OpenOrderExpectedPrice = deal.OpenOrderExpectedPrice,
                CloseTradeId = deal.CloseTradeId,
                CloseOrderType = deal.CloseOrderType.ToString(),
                CloseOrderVolume = deal.CloseOrderVolume,
                CloseOrderExpectedPrice = deal.CloseOrderExpectedPrice,
                Direction = deal.Direction.ToString(),
                Volume = deal.Volume,
                Originator = deal.Originator.ToString(),
                OpenPrice = deal.OpenPrice,
                OpenFxPrice = deal.OpenFxPrice,
                ClosePrice = deal.ClosePrice,
                CloseFxPrice = deal.CloseFxPrice,
                Fpl = deal.Fpl,
                AdditionalInfo = deal.AdditionalInfo,
                PnlOfTheLastDay = deal.PnlOfTheLastDay,
            };
        }
    }
}
