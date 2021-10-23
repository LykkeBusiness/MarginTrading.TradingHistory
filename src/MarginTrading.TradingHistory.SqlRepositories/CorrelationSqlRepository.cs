// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class CorrelationSqlRepository : ICorrelationRepository
    {
        private const string TableName = "Correlations";
        
        private static readonly string GetColumns =
            string.Join(",", typeof(CorrelationEntity).GetProperties().Select(x => x.Name));
        
        private static readonly string GetFields =
            string.Join(",", typeof(CorrelationEntity).GetProperties().Select(x => "@" + x.Name));
        
        private readonly string _connectionString;

        public CorrelationSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            
            connectionString.InitializeSqlObject("dbo.Correlations.sql", log);
        }

        public async Task AddAsync(ICorrelation correlation)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var correlationEntity = CorrelationEntity.Create(correlation);
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})",
                    correlationEntity);   
            }
        }
    }
}
