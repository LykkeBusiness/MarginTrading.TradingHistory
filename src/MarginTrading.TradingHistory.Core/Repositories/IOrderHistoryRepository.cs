using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrderHistoryRepository
    {
        Task AddAsync(IOrderHistory order);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync();
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync(Func<IOrderHistory, bool> predicate);
    }
}
