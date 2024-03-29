﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Mappers;
using MarginTrading.TradingHistory.SqlRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    /// <summary>
    /// Expose all events associated with orders, optionally including related orders.
    /// </summary>
    [Authorize]
    [Route("api/order-events")]
    public class OrderEventsController : Controller, IOrderEventsApi
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        private readonly OrderHistoryForSupportQuery _orderHistoryForSupportQuery;
        
        public OrderEventsController(IOrdersHistoryRepository ordersHistoryRepository, OrderHistoryForSupportQuery orderHistoryForSupportQuery)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
            _orderHistoryForSupportQuery = orderHistoryForSupportQuery;
        }

        /// <summary>
        /// Get orders with optional filtering, optionally including related orders, and with pagination.
        /// If parentOrderId is passed, withRelated is ignored.
        /// </summary>
        [HttpPost, Route("")]
        [HttpPost, Route("by-pages")]
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<OrderEventWithAdditionalContract>> OrderHistoryByPages(
            [FromBody] OrderEventsFilterRequest filters, 
            [FromQuery] int? skip = 0, [FromQuery] int? take = 20, 
            [FromQuery] bool isAscending = false)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);
            
            var data = await _ordersHistoryRepository.GetHistoryByPagesAsync(
                accountId: filters?.AccountId, 
                assetPairId: filters?.AssetPairId,
                statuses: filters?.Statuses?.Select(x => x.ToType<OrderStatus>()).ToList(), 
                orderTypes: filters?.OrderTypes?.Select(x => x.ToType<OrderType>()).ToList(), 
                originatorTypes: filters?.OriginatorTypes?.Select(x => x.ToType<OriginatorType>()).ToList(),
                parentOrderId: filters?.ParentOrderId,
                createdTimeStart: filters?.CreatedTimeStart, 
                createdTimeEnd: filters?.CreatedTimeEnd, 
                modifiedTimeStart: filters?.ModifiedTimeStart, 
                modifiedTimeEnd: filters?.ModifiedTimeEnd,
                skip: skip,
                take: take,
                isAscending: isAscending,
                executedOrdersEssentialFieldsOnly: filters?.RequestType == OrderEventsRequestType.ExecutedOrders);

            return new Lykke.Contracts.Responses.PaginatedResponse<OrderEventWithAdditionalContract>(
                contents: data.Contents.Select(Convert).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        [HttpPost("/api/order-events/for-support")]
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<OrderEventForSupportContract>> OrderHistoryForSupport([FromBody]OrderEventsForSupportRequest request)
        {
            (request.Skip, request.Take) = PaginationUtils.ValidateSkipAndTake(request.Skip, request.Take);

            var result = await _orderHistoryForSupportQuery.Ask(request.FromContract());
            var items = result.Contents.Select(p => p.ToContract()).ToList();

            return new Lykke.Contracts.Responses.PaginatedResponse<OrderEventForSupportContract>(contents: items,
                start: result.Start, 
                size: result.Size, 
                totalSize: result.TotalSize);
        }

        /// <summary>
        /// Get order by Id, optionally including related orders.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet, Route("{orderId}")]
        public async Task<List<OrderEventWithAdditionalContract>> OrderById(string orderId,
            [FromQuery] OrderStatusContract? status = null)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ArgumentException("Order id must be set", nameof(orderId));
            }

            var history = await _ordersHistoryRepository.GetHistoryAsync(orderId, status?.ToType<OrderStatus>());

            return history.Select(Convert).ToList();
        }

        private static OrderEventWithAdditionalContract Convert(IOrderHistoryWithAdditional history)
        {
            if (history == null)
                return null;

            return new OrderEventWithAdditionalContract
            {
                Id = history.Id,
                AccountId = history.AccountId,
                AssetPairId = history.AssetPairId,
                ParentOrderId = history.ParentOrderId,
                PositionId = history.Id,
                Direction = history.Direction.ToType<OrderDirectionContract>(),
                Type = history.Type.ToType<OrderTypeContract>(),
                Status = history.Status.ToType<OrderStatusContract>(),
                FillType = history.FillType.ToType<OrderFillTypeContract>(),
                Originator = history.Originator.ToType<OriginatorTypeContract>(),
                CancellationOriginator = history.CancellationOriginator.ToType<OriginatorTypeContract>(),
                Volume = history.Volume,
                ExpectedOpenPrice = history.ExpectedOpenPrice,
                ExecutionPrice = history.ExecutionPrice,
                FxRate = history.FxRate,
                FxAssetPairId = history.FxAssetPairId,
                FxToAssetPairDirection = history.FxToAssetPairDirection.ToType<FxToAssetPairDirectionContract>(),
                ForceOpen = history.ForceOpen,
                ValidityTime = history.ValidityTime,
                CreatedTimestamp = history.CreatedTimestamp,
                ModifiedTimestamp = history.ModifiedTimestamp,
                Code = history.Code,
                ActivatedTimestamp = history.ActivatedTimestamp,
                ExecutionStartedTimestamp = history.ExecutionStartedTimestamp,
                ExecutedTimestamp = history.ExecutedTimestamp,
                CanceledTimestamp = history.CanceledTimestamp,
                Rejected = history.Rejected,
                TradingConditionId = history.TradingConditionId,
                AccountAssetId = history.AccountAssetId,
                EquivalentAsset = history.EquivalentAsset,
                EquivalentRate = history.EquivalentRate,
                RejectReason = history.RejectReason.ToType<OrderRejectReasonContract>(),
                RejectReasonText = history.RejectReasonText,
                Comment = history.Comment,
                ExternalOrderId = history.ExternalOrderId,
                ExternalProviderId = history.ExternalProviderId,
                MatchingEngineId = history.MatchingEngineId,
                LegalEntity = history.LegalEntity,
                UpdateType = history.UpdateType.ToType<OrderUpdateTypeContract>(),
                AdditionalInfo = history.AdditionalInfo,
                CorrelationId = history.CorrelationId,
                StopLoss = Map(history.StopLoss),
                TakeProfit = Map(history.TakeProfit),
                Spread = history.Spread,
                Commission = history.Commission,
                OnBehalf = history.OnBehalf,
                PendingOrderRetriesCount = history.PendingOrderRetriesCount,
            };
        }

        private static RelatedOrderExtendedInfoContract Map(RelatedOrderExtendedInfo order)
        {
            if (order == null)
                return null;
            
            return new RelatedOrderExtendedInfoContract
            {
                Id = order.Id,
                Type = order.Type.ToType<OrderTypeContract>(),
                Price = order.ExpectedOpenPrice,
                Status = order.Status.ToType<OrderStatusContract>(),
                ModifiedTimestamp = order.ModifiedTimestamp
            };
        }
    }
}













