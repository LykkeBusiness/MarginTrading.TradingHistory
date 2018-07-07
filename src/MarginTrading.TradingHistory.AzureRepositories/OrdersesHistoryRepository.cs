using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    public class OrdersesHistoryRepository : IOrdersHistoryRepository
    {
        private readonly INoSQLTableStorage<OrderHistoryEntity> _tableStorage;
        private readonly IConvertService _convertService;

        public OrdersesHistoryRepository(
            INoSQLTableStorage<OrderHistoryEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }

        public Task AddAsync(IOrderHistory order)
        {
            var entity = OrderHistoryEntity.Create(order);
            // ReSharper disable once RedundantArgumentDefaultValue
            //TODO: use event datetime
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity,
                DateTime.UtcNow, RowKeyDateTimeFormat.Iso);
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId, string assetPairId)
        {
            var entities = await _tableStorage.GetDataAsync(accountId);

            return entities.OrderByDescending(entity => entity.ModifiedTimestamp);
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync()
        {
            var entities = await _tableStorage.GetDataAsync();
            return entities.OrderByDescending(item => item.Timestamp);

        }

        public async Task<IOrderHistory> GetHistoryAsync(string orderId)
        {
            var entity = await _tableStorage.GetDataAsync(x => x.Id == orderId);
            return entity.SingleOrDefault();
        }
    }
}
