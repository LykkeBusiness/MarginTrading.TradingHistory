﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ApiLibrary.Validation;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/deals")]
    public class DealsController : Controller, IDealsApi
    {
        private readonly IDealsRepository _dealsRepository;
        private readonly IConvertService _convertService;
        private readonly ILog _log;

        public DealsController(
            IDealsRepository dealsRepository,
            IConvertService convertService, 
            ILog log)
        {
            _dealsRepository = dealsRepository;
            _convertService = convertService;
            _log = log;
        }

        /// <summary>
        /// Get deals with optional filtering 
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<DealContract>> List([FromQuery] string accountId, [FromQuery] string instrument,
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null)
        {
            var data = await _dealsRepository.GetAsync(accountId, instrument, closeTimeStart, closeTimeEnd);

            return data.Where(d => d != null)
                .Select(_convertService.Convert<IDealWithCommissionParams, DealContract>)
                .ToList();
        }

        /// <summary>
        /// Get deals total PnL with optional filtering by period
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="instrument">The instrument id</param>
        /// <param name="closeTimeStart"></param>
        /// <param name="closeTimeEnd"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("totalPnl")]
        public async Task<TotalPnlContract> GetTotalPnL([FromQuery] string accountId, [FromQuery] string instrument,
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null,
            [FromQuery] List<PositionDirectionContract> directions = null)
        {
            var totalPnl = await _dealsRepository.GetTotalPnlAsync(accountId, instrument,
                directions?.Select(x => x.ToType<PositionDirection>()).ToList(),
                closeTimeStart, closeTimeEnd);

            return new TotalPnlContract {Value = totalPnl};
        }

        /// <summary>
        /// Get total profit of deals with filtering by set of days
        /// </summary>
        /// <param name="accountId">The account id</param>
        /// <param name="days">The days array</param>
        /// <returns></returns>
        [HttpGet]
        [Route("totalProfit")]
        public async Task<TotalProfitContract> GetTotalProfit(string accountId, DateTime[] days)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                await _log.WriteWarningAsync(
                    nameof(DealsController), 
                    nameof(GetTotalProfit), 
                    null,
                    $"{nameof(accountId)} value is not valid");
                
                return TotalProfitContract.Empty();
            }

            if (days == null || days.Length == 0)
            {
                await _log.WriteWarningAsync(
                    nameof(DealsController), 
                    nameof(GetTotalProfit), 
                    null,
                    $"{nameof(days)} value is not valid");
                
                return TotalProfitContract.Empty();
            }

            var totalProfit = await _dealsRepository.GetTotalProfitAsync(accountId, days);

            return new TotalProfitContract {Value = totalProfit};
        }

        /// <summary> 
        /// Get deals with optional filtering and pagination 
        /// </summary>
        [HttpGet, Route("by-pages")]
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<DealContract>> ListByPages(
            [FromQuery] string accountId, [FromQuery] string instrument, 
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null,
            [FromQuery] int? skip = null, [FromQuery] int? take = null,
            [FromQuery] bool isAscending = false, [FromQuery] List<PositionDirectionContract> directions = null)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);
            
            var data = await _dealsRepository.GetByPagesAsync(accountId, instrument,
                directions?.Select(x => x.ToType<PositionDirection>()).ToList(),
                closeTimeStart, closeTimeEnd, skip: skip, take: take, isAscending: isAscending);

            return new Lykke.Contracts.Responses.PaginatedResponse<DealContract>(
                contents: data.Contents.Select(
                    _convertService.Convert<IDealWithCommissionParams, DealContract>).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        /// <summary> 
        /// Get deals with optional filtering and pagination 
        /// </summary>
        [HttpGet, Route("aggregated")]
        [ValidateModel]
        public async Task<Lykke.Contracts.Responses.PaginatedResponse<AggregatedDealContract>> GetAggregated(
            [FromQuery] [Required] string accountId, [FromQuery] string instrument,
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null,
            [FromQuery] int? skip = null, [FromQuery] int? take = null,
            [FromQuery] bool isAscending = false, [FromQuery] List<PositionDirectionContract> directions = null)
        {
            if(string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentNullException(nameof(accountId), $"{nameof(accountId)} must be provided");
            }

            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            var data = await _dealsRepository.GetAggregated(accountId, instrument,
                directions?.Select(x => x.ToType<PositionDirection>()).ToList(),
                closeTimeStart, closeTimeEnd, skip: skip, take: take, isAscending: isAscending);

            return new Lykke.Contracts.Responses.PaginatedResponse<AggregatedDealContract>(
                contents: data.Contents.Select(
                    _convertService.Convert<IAggregatedDeal, AggregatedDealContract>).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
        }

        /// <summary>
        /// Get deal by Id
        /// </summary>
        /// <param name="dealId"></param>
        /// <returns></returns>
        [HttpGet, Route("{dealId}")]
        public async Task<DealContract> ById(string dealId)
        {
            if (string.IsNullOrWhiteSpace(dealId))
            {
                throw new ArgumentException("Deal id must be set", nameof(dealId));
            }
            
            var deal = await _dealsRepository.GetAsync(dealId);

            return deal == null ? null : _convertService.Convert<IDealWithCommissionParams, DealContract>(deal);
        }
        
        [HttpGet, Route("{dealId}/details")]
        public async Task<DealDetailsContract> GetDetails(string dealId)
        {
            if (string.IsNullOrWhiteSpace(dealId))
            {
                throw new ArgumentException("Deal id must be set", nameof(dealId));
            }

            var dealDetailsModel = await _dealsRepository.GetDetailsAsync(dealId);
            
            return dealDetailsModel == null ? null : _convertService.Convert<IDealDetails, DealDetailsContract>(dealDetailsModel);
        }
    }
}
