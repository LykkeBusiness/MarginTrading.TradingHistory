// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        Task<IDealWithCommissionParams> GetAsync(string id);
        [ItemCanBeNull]
        Task<IDealDetails> GetDetailsAsync(string id);
        Task<PaginatedResponse<IDealWithCommissionParams>> GetByPagesAsync(string accountId,
            string assetPairId, List<PositionDirection> directions,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true);

        Task<PaginatedResponse<IAggregatedDeal>> GetAggregated(string accountId,
            string assetPairId, List<PositionDirection> directions,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true);

        Task<IEnumerable<IDealWithCommissionParams>> GetAsync([CanBeNull] string accountId,
            [CanBeNull] string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null);
            
        Task<decimal> GetTotalPnlAsync([CanBeNull] string accountId,
            [CanBeNull] string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null);
        
        Task<decimal> GetTotalProfitAsync(string accountId, DateTime[] days);
    }
}
