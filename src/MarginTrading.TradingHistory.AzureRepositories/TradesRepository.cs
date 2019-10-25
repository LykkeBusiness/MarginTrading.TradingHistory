// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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

        public async Task<ITrade> GetAsync(string tradeId)
        {
            return (await _tableStorage.GetDataAsync(x => x.Id == tradeId)).SingleOrDefault();
        }

        public async Task<IEnumerable<ITrade>> GetByAccountAsync(string accountId, string assetPairId = null)
        {
            accountId.RequiredNotNullOrWhiteSpace(nameof(accountId));

            return await _tableStorage.GetDataAsync(accountId, x =>
                string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId);
        }

        public async Task<PaginatedResponse<ITrade>> GetByPagesAsync(string accountId, string assetPairId,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            var allData = await GetByAccountAsync(accountId, assetPairId);

            //TODO refactor before using azure impl
            var data = (isAscending
                    ? allData.OrderBy(x => x.OrderCreatedDate)
                    : allData.OrderByDescending(x => x.OrderCreatedDate))
                .ToList();
            var filtered = take.HasValue ? data.Skip(skip.Value).Take(take.Value).ToList() : data;
            
            return new PaginatedResponse<ITrade>(
                contents: filtered,
                start: skip ?? 0,
                size: filtered.Count,
                totalSize: data.Count
            );
        }

        public Task SetCancelledByAsync(string cancelledTradeId, string cancelledBy)
        {
            throw new System.NotImplementedException();
        }
    }
}
