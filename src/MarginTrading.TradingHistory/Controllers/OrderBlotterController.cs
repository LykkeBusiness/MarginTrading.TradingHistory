// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/order-blotter")]
    public class OrderBlotterController : Controller, IOrderBlotterApi
    {
        private readonly IOrdersHistoryRepository _ordersHistoryRepository;
        
        public OrderBlotterController(
            IOrdersHistoryRepository ordersHistoryRepository)
        {
            _ordersHistoryRepository = ordersHistoryRepository;
        }

        [HttpGet, Route("created-by-on-behalf")]
        public async Task<IEnumerable<string>> GetCreatedByOnBehalfList()
        {
            return await _ordersHistoryRepository.GetCreatedByOnBehalfListAsync();
        }

        [HttpGet]
        public async Task<PaginatedResponseContract<OrderForOrderBlotterContract>> Get(
            [FromQuery, Required] DateTime? relevanceTimestamp,
            [FromQuery] string accountIdOrName,
            [FromQuery] string assetName,
            [FromQuery] string createdBy,
            [FromQuery] List<OrderStatusContract> statuses,
            [FromQuery] List<OrderTypeContract> orderTypes,
            [FromQuery] List<OriginatorTypeContract> originatorTypes,
            [FromQuery] DateTime? createdOnFrom,
            [FromQuery] DateTime? createdOnTo,
            [FromQuery] DateTime? modifiedOnFrom,
            [FromQuery] DateTime? modifiedOnTo,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromQuery] OrderBlotterSortingColumnContract sortingColumn,
            [FromQuery] SortingOrderContract sortingOrder)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            var result = await _ordersHistoryRepository.GetOrderBlotterAsync(
                relevanceTimestamp.Value,
                accountIdOrName,
                assetName,
                createdBy,
                statuses?.Select(x => x.ToType<OrderStatus>()).ToList(),
                orderTypes?.Select(x => x.ToType<OrderType>()).ToList(),
                originatorTypes?.Select(x => x.ToType<OriginatorType>()).ToList(),
                createdOnFrom,
                createdOnTo,
                modifiedOnFrom,
                modifiedOnTo,
                skip,
                take,
                sortingColumn.ToType<OrderBlotterSortingColumn>(),
                sortingOrder.ToType<SortingOrder>());

            return new PaginatedResponseContract<OrderForOrderBlotterContract>(
                contents: result.Contents.Select(Convert).ToList(),
                start: result.Start, 
                size: result.Size, 
                totalSize: result.TotalSize);
        }
        
        private static OrderForOrderBlotterContract Convert(IOrderHistoryForOrderBlotterWithAdditionalData history)
        {
            var exchangeRate = history.FxRate == 0 ? 1 : 1 / history.FxRate;
            decimal? notional = null;
            decimal? notionalEUR = null;
            if (history.Status == OrderStatus.Executed && history.ExecutionPrice.HasValue)
            {
                notional = Math.Abs(history.Volume * history.ExecutionPrice.Value);
                notionalEUR = notional / exchangeRate;
            }
                
            return new OrderForOrderBlotterContract
            {
                AccountId = history.AccountId,
                AccountName = history.AccountName,
                CreatedBy = history.CreatedBy,
                InstrumentId = history.AssetPairId,
                Instrument = history.AssetName,
                Quantity = history.Volume,
                OrderType = history.Type.ToType<OrderTypeContract>(),
                OrderStatus = history.Status.ToType<OrderStatusContract>(),
                LimitStopPrice = history.ExpectedOpenPrice,
                TakeProfitPrice = history.TakeProfitPrice,
                StopLossPrice = history.StopLossPrice,
                Price = history.ExecutionPrice,
                Notional = notional,
                NotionalEur = notionalEUR,
                ExchangeRate = exchangeRate,
                Direction = history.Direction.ToType<OrderDirectionContract>(),
                Originator = history.Originator.ToType<OriginatorTypeContract>(),
                OrderId = history.Id,
                CreatedOn = history.CreatedTimestamp,
                ModifiedOn = history.ModifiedTimestamp,
                Validity = history.ValidityTime,
                OrderComment = history.Comment,
                Commission = history.Commission,
                OnBehalfFee = history.OnBehalfFee,
                Spread = history.Spread,
                ForcedOpen = history.ForceOpen
            };
        }
    }
}

