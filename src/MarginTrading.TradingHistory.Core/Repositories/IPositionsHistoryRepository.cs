// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IPositionsHistoryRepository
    {
        Task<List<IPositionHistory>> GetAsync(string accountId, string assetPairId, DateTime? eventDateFrom, DateTime? eventDateTo);
        
        Task<PaginatedResponse<IPositionHistory>> GetByPagesAsync(string accountId, string assetPairId, DateTime? eventDateFrom, DateTime? eventDateTo, int? skip = null, int? take = null);
        
        Task<List<IPositionHistory>> GetAsync(string id);
        
        Task AddAsync(IPositionHistory positionHistory, IDeal deal);
    }
}
