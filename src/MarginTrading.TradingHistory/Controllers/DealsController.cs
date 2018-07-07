using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
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
        /// <param name="accountId"></param>
        /// <param name="instrument"></param>
        /// <returns></returns>
        [HttpGet, Route("")]
        public async Task<List<DealContract>> List(string accountId, string instrument)
        {
            var data = await _dealsRepository.GetAsync(accountId, instrument);

            return data.Where(d => d != null).Select(Convert).ToList();
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
            return _convertService.Convert<IDeal, DealContract>(deal);
        }
    }
}
