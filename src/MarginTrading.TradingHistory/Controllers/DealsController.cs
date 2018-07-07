using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.Controllers
{
    public class DealsController : Controller
    {
        private readonly IDealsRepository _dealsRepository;
        private readonly IConvertService _convertService;

        public DealsController(IDealsRepository dealsRepository,
            IConvertService convertService)
        {
            _dealsRepository = dealsRepository;
            _convertService = convertService;
        }
        
        
    }
}
