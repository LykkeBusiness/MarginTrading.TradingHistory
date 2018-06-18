using MarginTrading.TradingHistory.BrokerBase.Settings;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.TradingHistory.BrokerBase.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly CurrentApplicationInfo _applicationInfo;

        public IsAliveController(CurrentApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new {
                ApplicationName = _applicationInfo.ApplicationName,
                Version = _applicationInfo.ApplicationVersion,
                Env = "",
            });
        }
    }
}
