using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface ITradesRepository
    {
        [ItemCanBeNull]
        Task<ITrade> GetAsync(string id);
        Task UpsertAsync(ITrade obj);
    }
}
