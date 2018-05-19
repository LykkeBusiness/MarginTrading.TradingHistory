using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrdersHistoryRepository
    {
        Task AddAsync(IOrderHistory order);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync();
        Task<IReadOnlyList<IOrderHistory>> GetHistoryAsync(string[] accountIds, DateTime? from, DateTime? to);
        Task<IEnumerable<IOrderHistory>> GetHistoryAsync(Func<IOrderHistory, bool> predicate);
    }
}
