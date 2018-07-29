using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IDealsRepository
    {
        [ItemCanBeNull]
        Task<IDeal> GetAsync(string id);
        Task<PaginatedResponse<IDeal>> GetByPagesAsync(string accountId, string assetPairId, 
            int? skip = null, int? take = null);
        Task<IEnumerable<IDeal>> GetAsync([CanBeNull] string accountId, [CanBeNull] string assetPairId);
        Task AddAsync(IDeal obj);
    }
}
