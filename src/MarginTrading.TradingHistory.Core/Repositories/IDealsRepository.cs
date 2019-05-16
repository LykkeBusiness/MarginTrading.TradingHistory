using System;
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
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true);

        Task<PaginatedResponse<IAggregatedDeal>> GetAggregated(string accountId, string assetPairId,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true);

        Task<IEnumerable<IDeal>> GetAsync([CanBeNull] string accountId, [CanBeNull] string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null);
        Task AddAsync(IDeal obj);
    }
}
