using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;
using MoreLinq;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    public class OrdersHistoryRepository : IOrdersHistoryRepository
    {
        private readonly INoSQLTableStorage<OrderHistoryEntity> _tableStorage;

        public OrdersHistoryRepository(INoSQLTableStorage<OrderHistoryEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task AddAsync(IOrderHistory order)
        {
            var entity = OrderHistoryEntity.Create(order);
            // ReSharper disable once RedundantArgumentDefaultValue
            //TODO: use event datetime
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity,
                DateTime.UtcNow, RowKeyDateTimeFormat.Iso);
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId, string assetPairId,
            bool withRelated = false)
        {
            var entities = await _tableStorage.GetDataAsync(x =>
                (string.IsNullOrWhiteSpace(accountId) || x.AccountId == accountId)
                || (string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId));

            var related = await _tableStorage.GetDataAsync(x => entities.Select(e => e.Id).Contains(x.ParentOrderId));

            return entities.Concat(related).OrderByDescending(entity => entity.ModifiedTimestamp);
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string orderId, bool withRelated = false)
        {
            var entities = await _tableStorage.GetDataAsync(x => x.Id == orderId
                                                                 || (withRelated && x.ParentOrderId == orderId));

            return entities;
        }
    }
}
