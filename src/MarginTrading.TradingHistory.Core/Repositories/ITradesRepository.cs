using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface ITradesRepository
    {
        [ItemCanBeNull]
        Task<Trade> GetAsync(string id);
        Task UpsertAsync(Trade obj);
    }
}
