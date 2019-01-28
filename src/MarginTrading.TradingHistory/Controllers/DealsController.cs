using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Common;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Route("api/deals")]
    public class DealsController : Controller, IDealsApi
    {
        private readonly IDealsRepository _dealsRepository;
        private readonly IConvertService _convertService;

        public DealsController(IDealsRepository dealsRepository,
            IConvertService convertService)
        {
            _dealsRepository = dealsRepository;
            _convertService = convertService;
        }

        /// <summary>
        /// Get deals with optional filtering 
        /// </summary>
        [HttpGet, Route("")]
        public async Task<List<DealContract>> List([FromQuery] string accountId, [FromQuery] string instrument,
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null)
        {
            var data = await _dealsRepository.GetAsync(accountId, instrument, closeTimeStart, closeTimeEnd);

            return data.Where(d => d != null).Select(Convert).ToList();
        }

        /// <summary> 
        /// Get deals with optional filtering and pagination 
        /// </summary>
        [HttpGet, Route("by-pages")]
        public async Task<PaginatedResponseContract<DealContract>> ListByPages(
            [FromQuery] string accountId, [FromQuery] string instrument, 
            [FromQuery] DateTime? closeTimeStart = null, [FromQuery] DateTime? closeTimeEnd = null,
            [FromQuery] int? skip = null, [FromQuery] int? take = null,
            [FromQuery] bool isAscending = false)
        {
            ApiValidationHelper.ValidatePagingParams(skip, take);
            
            var data = await _dealsRepository.GetByPagesAsync(accountId, instrument, 
                closeTimeStart, closeTimeEnd, skip: skip, take: take, isAscending: isAscending);

            return new PaginatedResponseContract<DealContract>(
                contents: data.Contents.Select(Convert).ToList(),
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

            return deal == null ? null : Convert(deal);
        }

        private DealContract Convert(IDeal deal)
        {
            return _convertService.Convert<IDeal, DealContract>(deal, opts => opts.ConfigureMap()
                .ForMember(x => x.Direction, 
                    o => o.ResolveUsing(z => z.Direction.ToType<PositionDirectionContract>())));
        }
    }
}
