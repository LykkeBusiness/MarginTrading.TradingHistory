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
    public class OrdersHistoryRepository : IOrdersHistoryRepository
    {
        private readonly INoSQLTableStorage<OrderHistoryEntity> _tableStorage;
        private readonly IConvertService _convertService;

        public OrdersHistoryRepository(
            INoSQLTableStorage<OrderHistoryEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }

        public Task AddAsync(OrderHistory order)
        {
            var entity = OrderHistoryEntity.Create(order);
            // ReSharper disable once RedundantArgumentDefaultValue
            //TODO: use event datetime
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity,
                DateTime.UtcNow, RowKeyDateTimeFormat.Iso);
        }

        public async Task<IReadOnlyList<OrderHistory>> GetHistoryAsync(string[] accountIds,
            DateTime? from, DateTime? to)
        {
            var entities = (await _tableStorage.WhereAsync(accountIds,
                    from ?? DateTime.MinValue, to?.Date.AddDays(1) ?? DateTime.MaxValue, ToIntervalOption.IncludeTo));
            
            return entities.Select(_convertService.Convert<OrderHistoryEntity, OrderHistory>)
                .OrderByDescending(entity => entity.CloseDate ?? entity.OpenDate ?? entity.CreateDate).ToList();
        }

        public async Task<IEnumerable<OrderHistory>> GetHistoryAsync()
        {
            var entities = await _tableStorage.GetDataAsync();
            return entities.OrderByDescending(item => item.Timestamp)
                .Select(_convertService.Convert<OrderHistoryEntity, OrderHistory>);

        }

        public async Task<IEnumerable<OrderHistory>> GetHistoryAsync(Func<OrderHistory, bool> predicate)
        {
            var entities = await _tableStorage.GetDataAsync(x =>
                predicate(_convertService.Convert<OrderHistoryEntity, OrderHistory>(x)));
            return entities.OrderByDescending(item => item.Timestamp)
                .Select(_convertService.Convert<OrderHistoryEntity, OrderHistory>);
        }
    }
}
