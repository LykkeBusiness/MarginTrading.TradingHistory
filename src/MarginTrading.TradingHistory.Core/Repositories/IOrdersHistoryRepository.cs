using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrdersHistoryRepository
    {
        Task AddAsync(IOrderHistory order);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId, string assetPairId, bool withRelated = false);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string orderId, bool withRelated = false);
    }
}
