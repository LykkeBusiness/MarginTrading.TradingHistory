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
    public class DealsSqlRepository : IDealsRepository
    {
        private const string TableName = "Deals";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [bigint] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[DealId] [nvarchar](64) NOT NULL,
[Created] [datetime] NOT NULL,
[AccountId] [nvarchar](64) NOT NULL,
[AssetPairId] [nvarchar](64) NOT NULL,
[OpenTradeId] [nvarchar] (64) NOT NULL,
[CloseTradeId] [nvarchar] (64) NULL,
[Direction] [nvarchar] (64) NOT NULL,
[Volume] [float] NULL,
[Originator] [nvarchar] (64) NOT NULL,
[OpenPrice] [float] NULL,
[OpenFxPrice] [float] NULL,
[ClosePrice] [float] NULL,
[CloseFxPrice] [float] NULL,
[Fpl] [float] NULL,
[AdditionalInfo] [nvarchar](MAX) NULL ,
INDEX IX_{0}_Base (DealId, AccountId, AssetPairId, Created)
);";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(IDeal).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(IDeal).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IDeal).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public DealsSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(DealsSqlRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }
        
        public async Task<IDeal> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} WHERE DealId = @id";
                var objects = await conn.QueryAsync<DealEntity>(query, new {id});
                
                return objects.FirstOrDefault();
            }
        }

        public async Task<PaginatedResponse<IDeal>> GetByPagesAsync(string accountId, string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null,
            int? skip = null, int? take = null)
        {
            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                              + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var paginationClause = $" ORDER BY [Oid] OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {accountId, assetPairId});
                var deals = (await gridReader.ReadAsync<DealEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
            
                return new PaginatedResponse<IDeal>(
                    contents: deals, 
                    start: skip ?? 0, 
                    size: deals.Count, 
                    totalSize: !take.HasValue ? deals.Count : totalCount
                );
            }
        }

        public async Task<IEnumerable<IDeal>> GetAsync(string accountId, string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE 1=1 "
                    + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                    + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId = @assetPairId")
                    + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                    + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
                
                var query = $"SELECT * FROM {TableName} {clause}";
                return await conn.QueryAsync<DealEntity>(query, new {accountId, assetPairId});
            }
        }

        public async Task AddAsync(IDeal obj)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    var entity = DealEntity.Create(obj);
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              "Entity <IDealHistory>: \n" +
                              obj.ToJson();
                    
                    _log?.WriteWarning(nameof(DealsSqlRepository), nameof(AddAsync), msg);
                    
                    throw new Exception(msg);
                }
            }
        }
    }
}
