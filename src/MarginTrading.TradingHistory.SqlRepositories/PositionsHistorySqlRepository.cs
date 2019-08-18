// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Extensions;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class PositionsHistorySqlRepository : IPositionsHistoryRepository
    {
        private const string TableName = "PositionsHistory";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(PositionsHistoryEntity).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(PositionsHistoryEntity).GetProperties().Select(x => "@" + x.Name));

        public PositionsHistorySqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            connectionString.InitializeSqlObject("dbo.Deals.sql", log);
            connectionString.InitializeSqlObject("dbo.SP_InsertDeal.sql", log);
            connectionString.InitializeSqlObject("dbo.PositionHistory.sql", log);
        }

        public async Task AddAsync(IPositionHistory positionHistory, IDeal deal)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }
                var transaction = conn.BeginTransaction();

                try
                {
                    var positionEntity = PositionsHistoryEntity.Create(positionHistory);
                    await conn.ExecuteAsync($"insert into {TableName} ({GetColumns}) values ({GetFields})",
                        positionEntity,
                        transaction);

                    if (deal != null)
                    {
                        await conn.ExecuteAsync("[dbo].[SP_InsertDeal]",
                            DealEntity.Create(deal),
                            transaction,
                            commandType: CommandType.StoredProcedure);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var msg = $"Error {ex.Message} \n" +
                              $"Entity <{nameof(IPositionHistory)}>: \n" +
                              positionHistory.ToJson() + " \n" +
                              $"Entity <{nameof(IDeal)}>: \n" +
                              deal?.ToJson();
                    
                    _log?.WriteError(nameof(PositionsHistorySqlRepository), nameof(AddAsync), 
                        new Exception(msg));
                    
                    throw;
                }
            }
        }

        public async Task<List<IPositionHistory>> GetAsync(string accountId, string assetPairId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var whereClause = "Where 1=1 " +
                                  (string.IsNullOrEmpty(accountId) ? "" : " And AccountId = @accountId") +
                                  (string.IsNullOrEmpty(assetPairId) ? "" : " And AssetPairId = @assetPairId");

                var query = $"SELECT * FROM {TableName} {whereClause}";
                var objects = await conn.QueryAsync<PositionsHistoryEntity>(query, new {accountId, assetPairId});
                
                return objects.Cast<IPositionHistory>().ToList();
            }
        }

        public async Task<PaginatedResponse<IPositionHistory>> GetByPagesAsync(string accountId, string assetPairId, 
            int? skip = null, int? take = null)
        {
            var whereClause = " WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId");
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var paginationClause = $" ORDER BY [Oid] OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {accountId,  assetPairId});
                var positionsHistoryEntities = (await gridReader.ReadAsync<PositionsHistoryEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
             
                return new PaginatedResponse<IPositionHistory>(
                    contents: positionsHistoryEntities, 
                    start: skip ?? 0, 
                    size: positionsHistoryEntities.Count, 
                    totalSize: totalCount
                );
            }
        }

        public async Task<List<IPositionHistory>> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} Where Id = @id";
                var objects = await conn.QueryAsync<PositionsHistoryEntity>(query, new {id});
                
                return objects.Cast<IPositionHistory>().ToList();
            }
        }
    }
}
