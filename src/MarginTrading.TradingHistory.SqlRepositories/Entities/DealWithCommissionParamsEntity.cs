// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.SqlRepositories.Entities
{
    public class DealWithCommissionParamsEntity : DealEntity, IDealWithCommissionParams
    {
        public decimal? OvernightFees { get; set; }
        public decimal? Commission { get; set; }
        public decimal? OnBehalfFee { get; set; }
        public decimal? Taxes { get; set; }
        

        public static DealWithCommissionParamsEntity Create(IDealWithCommissionParams deal)
        {
            return new DealWithCommissionParamsEntity
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
                
                OvernightFees = deal.OvernightFees,
                Commission = deal.Commission,
                OnBehalfFee = deal.OnBehalfFee,
                Taxes = deal.Taxes,
                CorrelationId = deal.CorrelationId
            };
        }
    }
}
