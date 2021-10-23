// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.TradingHistory.AzureRepositories.Entities;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.Core.Services;

namespace MarginTrading.TradingHistory.AzureRepositories
{
    public class CorrelationRepository : ICorrelationRepository
    {
        private readonly INoSQLTableStorage<CorrelationEntity> _tableStorage;
        private readonly IConvertService _convertService;
        
        public CorrelationRepository(INoSQLTableStorage<CorrelationEntity> tableStorage,
            IConvertService convertService)
        {
            _tableStorage = tableStorage;
            _convertService = convertService;
        }

        public Task AddAsync(ICorrelation correlation)
        {
            throw new System.NotImplementedException();
        }
    }
}
