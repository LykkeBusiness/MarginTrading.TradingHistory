// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.AzureRepositories.Entities
{
    public class DealEntity : AzureTableEntity, IDeal
    {
        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string DealId 
        {
            get => RowKey;
            set => RowKey = value;
        }
        
        public DateTime Created { get; set; }
        public string AssetPairId { get; set; }
        public string OpenTradeId { get; set; }
        public OrderType OpenOrderType { get; set; }
        public decimal OpenOrderVolume { get; set; }
        public decimal? OpenOrderExpectedPrice { get; set; }
        public string CloseTradeId { get; set; }
        public OrderType CloseOrderType { get; set; }
        public decimal CloseOrderVolume { get; set; }
        public decimal? CloseOrderExpectedPrice { get; set; }
        PositionDirection IDeal.Direction => (PositionDirection) Enum.Parse(typeof(PositionDirection), Direction);        
        public string Direction { get; set; }
        public decimal Volume { get; set; }
        OriginatorType IDeal.Originator => (OriginatorType) Enum.Parse(typeof(OriginatorType), Originator);
        public string Originator { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal OpenFxPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal CloseFxPrice { get; set; }
        public decimal Fpl { get; set; }
        public decimal PnlOfTheLastDay { get; set; }
        public string AdditionalInfo { get; set; }
        
        public decimal? OvernightFees { get; set; }
        public decimal? Commission { get; set; }
        public decimal? OnBehalfFee { get; set; }
        public decimal? Taxes { get; set; }
    }
}
