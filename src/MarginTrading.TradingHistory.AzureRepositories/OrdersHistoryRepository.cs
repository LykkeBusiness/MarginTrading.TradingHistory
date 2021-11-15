// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
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
        private readonly INoSQLTableStorage<OrderHistoryWithAdditionalEntity> _readTableStorage;

        public OrdersHistoryRepository(INoSQLTableStorage<OrderHistoryEntity> tableStorage,
            INoSQLTableStorage<OrderHistoryWithAdditionalEntity> readTableStorage)
        {
            throw new NotImplementedException("Need to be refactored!");
            _tableStorage = tableStorage;
            _readTableStorage = readTableStorage;
        }

        public Task AddAsync(IOrderHistory order, ITrade trade)
        {
            var entity = OrderHistoryEntity.Create(order);
            // ReSharper disable once RedundantArgumentDefaultValue
            //TODO: use event datetime
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity,
                DateTime.UtcNow, RowKeyDateTimeFormat.Iso);
        }

        public Task<PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>> GetOrderBlotterAsync(DateTime relevanceTimestamp, string accountIdOrName, string assetName, string createdBy,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes, DateTime? createdOnFrom, DateTime? createdOnTo,
            DateTime? modifiedOnFrom, DateTime? modifiedOnTo, int skip, int take, OrderBlotterSortingColumn sortingColumn, SortingOrder sortingOrder)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IOrderHistoryWithAdditional>> GetHistoryAsync(string orderId,
            OrderStatus? status = null)
        {
            var entities = await _readTableStorage.GetDataAsync(x => x.Id == orderId
                                                                 || status == null || x.Status == status);

            return entities;
        }

        public async Task<PaginatedResponse<IOrderHistoryWithAdditional>> GetHistoryByPagesAsync(string accountId, string assetPairId,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes,
            string parentOrderId = null,
            DateTime? createdTimeStart = null, DateTime? createdTimeEnd = null,
            DateTime? modifiedTimeStart = null, DateTime? modifiedTimeEnd = null,
            int? skip = null, int? take = null, bool isAscending = true,
            bool executedOrdersEssentialFieldsOnly = false)
        {
            //TODO refactor before using azure impl
            
            var entities = await _readTableStorage.GetDataAsync(x =>
                (string.IsNullOrWhiteSpace(accountId) || x.AccountId == accountId)
                && (string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId)
                && (string.IsNullOrEmpty(parentOrderId) || x.ParentOrderId == parentOrderId)
                && (statuses == null || statuses.Count == 0 || statuses.Any(s => s == x.Status))
                && (orderTypes == null || orderTypes.Count == 0 || orderTypes.Any(s => s == x.Type))
                && (originatorTypes == null || originatorTypes.Count == 0 || originatorTypes.Any(s => s == x.Originator))
                && (createdTimeStart == null || x.CreatedTimestamp >= createdTimeStart)
                && (createdTimeEnd == null || x.CreatedTimestamp < createdTimeEnd)
                && (modifiedTimeStart == null || x.ModifiedTimestamp >= modifiedTimeStart)
                && (modifiedTimeEnd == null || x.ModifiedTimestamp < modifiedTimeEnd));

            var data = (isAscending
                    ? entities.OrderBy(x => x.CreatedTimestamp)
                    : entities.OrderByDescending(x => x.CreatedTimestamp))
                .ToList();
            var filtered = take.HasValue ? data.Skip(skip.Value).Take(take.Value).ToList() : data;
            
            return new PaginatedResponse<IOrderHistoryWithAdditional>(
                contents: filtered,
                start: skip ?? 0,
                size: filtered.Count,
                totalSize: data.Count
            );
        }
    }
}
