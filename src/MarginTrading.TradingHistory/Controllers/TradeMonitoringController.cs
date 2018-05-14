using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    [Authorize]
    [Route("api/trade/")]
    public class TradeMonitoringController : Controller, ITradeMonitoringReadingApi
    {
        private readonly IConvertService _convertService;

        public TradeMonitoringController(
            IConvertService convertService)
        {
            _convertService = convertService;
        }

        
    }
}
