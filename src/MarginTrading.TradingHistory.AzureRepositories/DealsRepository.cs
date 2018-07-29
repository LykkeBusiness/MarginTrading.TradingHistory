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
    public class DealsRepository : IDealsRepository
    {
        private readonly INoSQLTableStorage<DealEntity> _tableStorage;

        private readonly IConvertService _convertService;
        
        public DealsRepository(INoSQLTableStorage<DealEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }
        
        public async Task<IDeal> GetAsync(string id)
        {
            return (await _tableStorage.GetDataAsync(x => x.DealId == id)).SingleOrDefault();
        }

        public async Task<PaginatedResponse<IDeal>> GetByPagesAsync(string accountId, string assetPairId, 
            int? skip = null, int? take = null)
        {
            var allData = await GetAsync(accountId, assetPairId);

            //TODO refactor before using azure impl
            var data = allData.OrderBy(x => x.Created).ToList();
            var filtered = take.HasValue ? data.Skip(skip.Value).Take(take.Value).ToList() : data;
            
            return new PaginatedResponse<IDeal>(
                contents: filtered,
                start: skip ?? 0,
                size: filtered.Count,
                totalSize: data.Count
            );
        }

        public async Task<IEnumerable<IDeal>> GetAsync(string accountId, string assetPairId)
        {
            return string.IsNullOrWhiteSpace(accountId)
                ? await _tableStorage.GetDataAsync(x =>
                    (string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId))
                : await _tableStorage.GetDataAsync(accountId, x => 
                    (string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId));
        }

        public Task AddAsync(IDeal obj)
        {
            return _tableStorage.InsertAsync(_convertService.Convert<IDeal, DealEntity>(obj));
        }
    }
}
