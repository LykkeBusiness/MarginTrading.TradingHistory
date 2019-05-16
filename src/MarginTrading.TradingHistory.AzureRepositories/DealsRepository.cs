using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core;
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
            DateTime? closeTimeStart, DateTime? closeTimeEnd, int? skip = null, int? take = null,
            bool isAscending = true)
        {
            var allData = await GetAsync(accountId, assetPairId, closeTimeStart, closeTimeEnd);

            //TODO refactor before using azure impl
            var data = (isAscending
                    ? allData.OrderBy(x => x.Created)
                    : allData.OrderByDescending(x => x.Created))
                .ToList();
            var filtered = take.HasValue ? data.Skip(skip.Value).Take(take.Value).ToList() : data;
            
            return new PaginatedResponse<IDeal>(
                contents: filtered,
                start: skip ?? 0,
                size: filtered.Count,
                totalSize: data.Count
            );
        }

        public async Task<IEnumerable<IDeal>> GetAsync(string accountId, string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            bool Predicate(DealEntity x) => (string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId) 
                                            && (closeTimeStart == null || x.Created >= closeTimeStart) 
                                            && (closeTimeEnd == null || x.Created < closeTimeEnd);

            return string.IsNullOrWhiteSpace(accountId)
                ? await _tableStorage.GetDataAsync((Func<DealEntity, bool>) Predicate)
                : await _tableStorage.GetDataAsync(accountId, (Func<DealEntity, bool>) Predicate);
        }

        public Task AddAsync(IDeal obj)
        {
            return _tableStorage.InsertAsync(_convertService.Convert<IDeal, DealEntity>(obj));
        }

        public async Task<PaginatedResponse<IAggregatedDeal>> GetAggregated(string accountId, string assetPairId, DateTime? closeTimeStart, DateTime? closeTimeEnd, int? skip = null, int? take = null, bool isAscending = true)
        {
            var allData = await GetAsync(accountId, assetPairId, closeTimeStart, closeTimeEnd);

            var groupedData = allData
                .GroupBy(k => new { k.AccountId, k.AssetPairId })
                .Select(g => new AggregatedDeal(
                    g.Key.AccountId,
                    g.Key.AssetPairId,
                    g.Sum(v => v.Volume),
                    g.Sum(v => v.Fpl),
                    g.Sum(v => v.Fpl / v.CloseFxPrice),
                    g.Sum(v => v.PnlOfTheLastDay),
                    g.Sum(v => v.OvernightFees),
                    g.Sum(v => v.Commission),
                    g.Sum(v => v.OnBehalfFee),
                    g.Sum(v => v.Taxes)));

            skip = skip ?? 0;

            var filtered = take.HasValue ? groupedData.Skip(skip.Value).Take(take.Value).ToList() : groupedData.ToList();

            return new PaginatedResponse<IAggregatedDeal>(
                contents: filtered,
                start: skip.Value,
                size: filtered.Count,
                totalSize: groupedData.Count());
        }
    }
}
