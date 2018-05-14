using System.Threading.Tasks;

namespace MarginTrading.TradingHistory.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
