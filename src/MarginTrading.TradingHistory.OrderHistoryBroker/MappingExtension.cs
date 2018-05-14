﻿using MarginTrading.Contract.BackendContracts;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    public static class MappingExtension
    {
        public static IOrderHistory ToOrderHistoryDomain(this OrderFullContract src)
        {
            var orderContract = new OrderHistory
            {
                Id = src.Id,
                Code = src.Code,
                ClientId = src.ClientId,
                AccountId = src.AccountId,
                TradingConditionId = src.TradingConditionId,
                AccountAssetId = src.AccountAssetId,
                Instrument = src.Instrument,
                Type = src.Type.ToType<OrderDirection>(),
                CreateDate = src.CreateDate,
                OpenDate = src.OpenDate,
                CloseDate = src.CloseDate,
                ExpectedOpenPrice = src.ExpectedOpenPrice,
                OpenPrice = src.OpenPrice,
                ClosePrice = src.ClosePrice,
                QuoteRate = src.QuoteRate,
                AssetAccuracy = src.AssetAccuracy,
                Volume = src.Volume,
                TakeProfit = src.TakeProfit,
                StopLoss = src.StopLoss,
                CommissionLot = src.CommissionLot,
                OpenCommission = src.OpenCommission,
                CloseCommission = src.CloseCommission,
                SwapCommission = src.SwapCommission,
                StartClosingDate = src.StartClosingDate,
                Status = src.Status.ToType<OrderStatus>(),
                CloseReason = src.CloseReason.ToType<OrderCloseReason>(),
                FillType = src.FillType.ToType<OrderFillType>(),
                RejectReason = src.RejectReason.ToType<OrderRejectReason>(),
                RejectReasonText = src.RejectReasonText,
                Comment = src.Comment,
                MatchedVolume = src.MatchedVolume,
                MatchedCloseVolume = src.MatchedCloseVolume,
                Fpl = src.Fpl,
                PnL = src.PnL,
                InterestRateSwap = src.InterestRateSwap,
                MarginInit = src.MarginInit,
                MarginMaintenance = src.MarginMaintenance,
                EquivalentAsset = src.EquivalentAsset,
                OpenPriceEquivalent = src.OpenPriceEquivalent,
                ClosePriceEquivalent = src.ClosePriceEquivalent,
                OrderUpdateType = src.OrderUpdateType.ToType<OrderUpdateType>(),
                OpenExternalOrderId = src.OpenExternalOrderId,
                OpenExternalProviderId = src.OpenExternalProviderId,
                CloseExternalOrderId = src.CloseExternalOrderId,
                CloseExternalProviderId = src.CloseExternalProviderId,
                MatchingEngineMode = src.MatchingEngineMode.ToType<MatchingEngineMode>(),
                LegalEntity = src.LegalEntity,
            };

            foreach (var order in src.MatchedOrders)
            {
                orderContract.MatchedOrders.Add(order.ToDomain());
            }

            foreach (var order in src.MatchedCloseOrders)
            {
                orderContract.MatchedCloseOrders.Add(order.ToDomain());
            }

            return orderContract;
        }
        
        public static MatchedOrder ToDomain(this MatchedOrderBackendContract src)
        {
            return new MatchedOrder
            {
                OrderId = src.OrderId,
                MarketMakerId = src.MarketMakerId,
                LimitOrderLeftToMatch = src.LimitOrderLeftToMatch,
                Volume = src.Volume,
                Price = src.Price,
                MatchedDate = src.MatchedDate
            };
        }
    }
}
