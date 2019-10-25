// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class TradesSqlRepository : ITradesRepository
    {
        public const string TableName = "Trades";

        private readonly string _connectionString;
        private readonly ILog _log;

        public static readonly string GetColumns =
            string.Join(",", typeof(ITrade).GetProperties().Select(x => x.Name));

        public static readonly string GetFields =
            string.Join(",", typeof(ITrade).GetProperties().Select(x => "@" + x.Name));

        public TradesSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            connectionString.InitializeSqlObject("dbo.Trades.sql", log);
        }

        public async Task<ITrade> GetAsync(string tradeId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} WHERE Id = @tradeId";
                var objects = await conn.QueryAsync<TradeEntity>(query, new {tradeId});
                
                return objects.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<ITrade>> GetByAccountAsync(string accountId, string assetPairId = null)
        {
            accountId.RequiredNotNullOrWhiteSpace(nameof(accountId));
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE 1=1 "
                             + $" AND AccountId = @{nameof(accountId)}"
                             + (string.IsNullOrWhiteSpace(assetPairId) ? "" : $" AND AssetPairId = @{nameof(assetPairId)}");
                
                var query = $"SELECT * FROM {TableName} {clause}";
                return await conn.QueryAsync<TradeEntity>(query, new {accountId, assetPairId});
            }
        }

        public async Task<PaginatedResponse<ITrade>> GetByPagesAsync(string accountId, string assetPairId,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [TradeTimestamp] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {accountId, assetPairId});
                var trades = (await gridReader.ReadAsync<TradeEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
             
                return new PaginatedResponse<ITrade>(
                    contents: trades, 
                    start: skip ?? 0, 
                    size: trades.Count, 
                    totalSize: totalCount
                );
            }
        }
        
        public async Task SetCancelledByAsync(string cancelledTradeId, string cancelledBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    $"UPDATE {TableName} SET CancelledBy = @cancelledBy WHERE Id = @cancelledTradeId",
                    new {cancelledTradeId, cancelledBy});
            }
        }
    }
}
