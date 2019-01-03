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
        private const string TableName = "Trades";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [bigint] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[Id] [nvarchar](64) NOT NULL,
[AccountId] [nvarchar](64) NOT NULL,
[OrderId] [nvarchar](64) NOT NULL,
[AssetPairId] [nvarchar] (64) NOT NULL,
[OrderCreatedDate] [datetime] NOT NULL,
[OrderType] [nvarchar] (64) NOT NULL,
[Type] [nvarchar] (64) NOT NULL,
[Originator] [nvarchar] (64) NOT NULL,
[TradeTimestamp] [datetime] NOT NULL,
[Price] [float] NULL,
[Volume] [float] NULL,
[OrderExpectedPrice] [float] NULL,
[FxRate] [float] NULL,
[AdditionalInfo] [nvarchar] (MAX) NULL,
[CancelledBy] [nvarchar] (64) NULL,
INDEX IX_{0}_Base (AccountId, AssetPairId)
);";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(ITrade).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(ITrade).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(ITrade).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public TradesSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(TradesSqlRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }
        
        
        public async Task AddAsync(ITrade obj)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    var entity = TradeEntity.Create(obj);
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              "Entity <ITradeHistory>: \n" +
                              obj.ToJson();
                    
                    _log?.WriteWarning(nameof(TradesSqlRepository), nameof(AddAsync), msg);
                    
                    throw new Exception(msg);
                }
            }
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
            int? skip = null, int? take = null)
        {
            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId");
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var paginationClause = $" ORDER BY [Oid] OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {accountId, assetPairId});
                var trades = (await gridReader.ReadAsync<TradeEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
             
                return new PaginatedResponse<ITrade>(
                    contents: trades, 
                    start: skip ?? 0, 
                    size: trades.Count, 
                    totalSize: !take.HasValue ? trades.Count : totalCount
                );
            }
        }
        
        public async Task SetCancelledByAsync(string cancelledTradeId, string cancelledBy)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(
                    $"UPDATE {TableName} SET CancelledBy = @cancelledBy WHERE Id = @cancelledTradeId",
                    new {cancelledOrderId = cancelledTradeId, cancelledBy});
            }
        }
    }
}
