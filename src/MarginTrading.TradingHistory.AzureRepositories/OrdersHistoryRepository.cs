﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;

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
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity,
                entity.UpdateTimestamp, RowKeyDateTimeFormat.Iso);
        }

        public async Task<IReadOnlyList<IOrderHistory>> GetHistoryAsync(string[] accountIds,
            DateTime? from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(accountIds,
                    from ?? DateTime.MinValue, to?.Date.AddDays(1) ?? DateTime.MaxValue, ToIntervalOption.IncludeTo))
                .OrderByDescending(entity => entity.CloseDate ?? entity.OpenDate ?? entity.CreateDate).ToList();
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync()
        {
            var entities = (await _tableStorage.GetDataAsync()).OrderByDescending(item => item.Timestamp);

            return entities;
        }
    }
}
