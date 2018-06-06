using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IPositionsHistoryRepository
    {
        Task<IEnumerable<IPositionHistory>> GetAsync(string accountId, string assetPairId);
        [ItemCanBeNull]
        Task<IPositionHistory> GetAsync(string id);
        Task AddAsync(IPositionHistory obj);
    }
}
