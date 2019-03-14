﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IPositionsHistoryRepository
    {
        Task<List<IPositionHistory>> GetAsync(string accountId, string assetPairId);
        
        Task<PaginatedResponse<IPositionHistory>> GetByPagesAsync(string accountId, string assetPairId, 
            int? skip = null, int? take = null);
        
        Task<List<IPositionHistory>> GetAsync(string id);
        
        Task TryAddAsync(IPositionHistory obj);
    }
}
