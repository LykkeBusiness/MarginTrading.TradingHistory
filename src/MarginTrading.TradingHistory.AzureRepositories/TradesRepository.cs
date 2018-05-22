using System;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    internal class TradesRepository : ITradesRepository
    {
        private readonly INoSQLTableStorage<TradeEntity> _tableStorage;

        private readonly IConvertService _convertService;

        public TradesRepository(INoSQLTableStorage<TradeEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }

        public async Task<Trade> GetAsync(string id)
        {
            var data = await _tableStorage.GetDataAsync(TradeEntity.GetPartitionKey(id), TradeEntity.GetRowKey());
            return _convertService.Convert<TradeEntity, Trade>(data);
        }

        // todo: use internal models instead of entity in the repo api
        public async Task UpsertAsync(Trade obj)
        {
            var entity = _convertService.Convert<Trade, TradeEntity>(obj, o => o.ConfigureMap(MemberList.Source));
            
            entity.TradeTimestamp = DateTime.UtcNow;
            entity.Timestamp = DateTimeOffset.UtcNow;
            entity.PartitionKey = TradeEntity.GetPartitionKey(obj.OrderId);
            entity.RowKey = TradeEntity.GetRowKey();
                
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
