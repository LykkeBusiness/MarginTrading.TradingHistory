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
    public class TradesRepository : ITradesRepository
    {
        private readonly INoSQLTableStorage<TradeEntity> _tableStorage;

        private readonly IConvertService _convertService;
        
        public TradesRepository(INoSQLTableStorage<TradeEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }
        
        public Task AddAsync(ITrade obj)
        {
            return _tableStorage.InsertAsync(_convertService.Convert<ITrade, TradeEntity>(obj));
        }

        public async Task<ITrade> GetAsync(string tradeId)
        {
            return (await _tableStorage.GetDataAsync(x => x.Id == tradeId)).SingleOrDefault();
        }

        public async Task<IEnumerable<ITrade>> GetAsync(string orderId, string positionId)
        {
            return await _tableStorage.GetDataAsync(x => x.OrderId == orderId && x.PositionId == positionId);
        }
    }
}
