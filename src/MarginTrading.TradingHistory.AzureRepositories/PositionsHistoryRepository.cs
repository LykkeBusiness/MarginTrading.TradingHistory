// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    internal class PositionsHistoryRepository : IPositionsHistoryRepository
    {
        private readonly INoSQLTableStorage<PositionHistoryEntity> _tableStorage;

        private readonly IConvertService _convertService;

        public PositionsHistoryRepository(INoSQLTableStorage<PositionHistoryEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }

        public async Task<List<IPositionHistory>> GetAsync(string accountId, string assetPairId, DateTime? eventDate)
        {
            var predicate = new Func<PositionHistoryEntity, bool>(p =>
                (string.IsNullOrEmpty(assetPairId) || p.AssetPairId == accountId) && (eventDate == null || p.HistoryTimestamp == eventDate));

            return (string.IsNullOrEmpty(accountId)
                    ? await _tableStorage.GetDataAsync(predicate)
                    : await _tableStorage.GetDataAsync(accountId, predicate))
                .Cast<IPositionHistory>().ToList();
        }

        public async Task<PaginatedResponse<IPositionHistory>> GetByPagesAsync(string accountId, string assetPairId, DateTime? eventDate,
            int? skip = null, int? take = null)
        {
            var allData = await GetAsync(accountId, assetPairId, eventDate);

            //TODO refactor before using azure impl
            var data = allData.OrderBy(x => x.HistoryTimestamp).ToList();
            var filtered = take.HasValue ? data.Skip(skip.Value).Take(take.Value).ToList() : data;
            
            return new PaginatedResponse<IPositionHistory>(
                contents: filtered,
                start: skip ?? 0,
                size: filtered.Count,
                totalSize: data.Count
            );
        }

        public async Task<List<IPositionHistory>> GetAsync(string id)
        {
            return (await _tableStorage.GetDataAsync(p => p.DealId == id))
                .Cast<IPositionHistory>().ToList();
        }

        public async Task AddAsync(IPositionHistory positionHistory, IDeal deal)
        {
            var entity =
                _convertService.Convert<IPositionHistory, PositionHistoryEntity>(positionHistory,
                    o => o.ConfigureMap(MemberList.Source));
            
            entity.Timestamp = DateTimeOffset.UtcNow;
                
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
