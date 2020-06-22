// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        
        public async Task<IDealWithCommissionParams> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResponse<IDealWithCommissionParams>> GetByPagesAsync(string accountId,
            string assetPairId,
            DateTime? closeTimeStart, DateTime? closeTimeEnd, int? skip = null, int? take = null,
            bool isAscending = true)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IDealWithCommissionParams>> GetAsync(string accountId, string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> GetTotalPnlAsync(string accountId, string assetPairId, DateTime? closeTimeStart = null,
            DateTime? closeTimeEnd = null)
        {
            throw new NotImplementedException();
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
                .Select(g => new AggregatedDeal
                {
                    AccountId = g.Key.AccountId,
                    AssetPairId = g.Key.AssetPairId,
                    Volume = g.Sum(v => v.Volume),
                    Fpl = g.Sum(v => v.Fpl),
                    FplTc = g.Sum(v => v.Fpl / v.CloseFxPrice),
                    PnlOfTheLastDay = g.Sum(v => v.PnlOfTheLastDay),
                    OvernightFees = g.Sum(v => v.OvernightFees),
                    Commission = g.Sum(v => v.Commission),
                    OnBehalfFee = g.Sum(v => v.OnBehalfFee),
                    Taxes = g.Sum(v => v.Taxes),
                    DealsCount = g.Count()
                });

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
