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

        public async Task<IEnumerable<IPositionHistory>> GetAsync(string accountId, string assetPairId)
        {
            var predicate = new Func<PositionHistoryEntity, bool>(p =>
                (string.IsNullOrEmpty(assetPairId) || p.AssetPairId == accountId) &&
                p.HistoryType == PositionHistoryType.Close || p.HistoryType == PositionHistoryType.PartiallyClose);
            
            return string.IsNullOrEmpty(accountId)
                ? await _tableStorage.GetDataAsync(predicate)
                : await _tableStorage.GetDataAsync(accountId, predicate);
        }

        public async Task<IPositionHistory> GetAsync(string id)
        {
            return (await _tableStorage.GetDataAsync(p => p.DealId == id))
                .SingleOrDefault();
        }

        public async Task AddAsync(IPositionHistory positionHistory)
        {
            var entity =
                _convertService.Convert<IPositionHistory, PositionHistoryEntity>(positionHistory,
                    o => o.ConfigureMap(MemberList.Source));
            
            entity.Timestamp = DateTimeOffset.UtcNow;
                
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
