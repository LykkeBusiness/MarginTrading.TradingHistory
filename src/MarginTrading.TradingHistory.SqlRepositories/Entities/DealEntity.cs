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
        public string CloseTradeId { get; set; }
        public decimal Volume { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal Fpl { get; set; }
        public string AdditionalInfo { get; set; }

        public static DealEntity Create(IDeal deal)
        {
            return new DealEntity
            {
                DealId = deal.DealId,
                Created = deal.Created,
                AccountId = deal.AccountId,
                AssetPairId = deal.AssetPairId,
                OpenTradeId = deal.OpenTradeId,
                CloseTradeId = deal.CloseTradeId,
                Volume = deal.Volume,
                OpenPrice = deal.OpenPrice,
                OpenFxPrice = deal.OpenFxPrice,
                ClosePrice = deal.ClosePrice,
                CloseFxPrice = deal.CloseFxPrice,
                Fpl = deal.Fpl,
                AdditionalInfo = deal.AdditionalInfo,
            };
        }
    }
}
