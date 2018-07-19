using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface ITradesRepository
    {
        Task AddAsync(ITrade obj);
        Task<ITrade> GetAsync(string tradeId);
        Task<IEnumerable<ITrade>> GetByAccountAsync([NotNull] string accountId, [CanBeNull] string assetPairId = null);
        Task<PaginatedResponse<ITrade>> GetByPagesAsync(string accountId, string assetPairId, 
            int? skip = null, int? take = null);
    }
}
