using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface ILogRepository
    {
        Task Insert(ILogEntity log);
    }
}