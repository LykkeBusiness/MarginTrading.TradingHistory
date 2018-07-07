using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;

namespace MarginTrading.TradingHistory.OrderHistoryBroker
{
    internal class RepositoryAggregator : IOrderHistoryRepository
    {
        private readonly List<IOrderHistoryRepository> _repositories;

        public RepositoryAggregator(IEnumerable<IOrderHistoryRepository> repositories)
        {
            _repositories = new List<IOrderHistoryRepository>();
            _repositories.AddRange(repositories);
        }

        public async Task AddAsync(IOrderHistory report)
        {
            foreach (var item in _repositories)
            {
                await item.AddAsync(report);
            }
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(Func<IOrderHistory, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
