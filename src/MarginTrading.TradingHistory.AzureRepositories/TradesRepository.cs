using System.Threading.Tasks;
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

        public async Task<ITrade> GetAsync(string id)
        {
            var data = await _tableStorage.GetDataAsync(TradeEntity.GetPartitionKey(id), TradeEntity.GetRowKey());
            return _convertService.Convert<TradeEntity, ITrade>(data);
        }

        // todo: use internal models instead of entity in the repo api
        public async Task UpsertAsync(ITrade obj)
        {
            var entity = _convertService.Convert<ITrade, TradeEntity>(obj);
            await _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
