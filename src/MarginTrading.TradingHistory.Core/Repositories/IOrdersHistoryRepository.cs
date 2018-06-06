using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface IOrdersHistoryRepository
    {
        Task AddAsync(OrderHistory order);
        Task<IEnumerable<OrderHistory>> GetHistoryAsync();
        Task<IReadOnlyList<OrderHistory>> GetHistoryAsync(string[] accountIds, DateTime? from, DateTime? to);
        Task<IEnumerable<OrderHistory>> GetHistoryAsync(Func<OrderHistory, bool> predicate);
    }
}
