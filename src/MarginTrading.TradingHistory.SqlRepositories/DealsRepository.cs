using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class DealsRepository : IDealsRepository
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
[Volume] [float] NULL,
[OpenPrice] [float] NULL,
[OpenFxPrice] [float] NULL,
[ClosePrice] [float] NULL,
[CloseFxPrice] [float] NULL,
[Fpl] [float] NULL,
[AdditionalInfo] [nvarchar](MAX) NULL,
CONSTRAINT IX_DealHistory1 NONCLUSTERED (OpenTradeId, CloseTradeId),
CONSTRAINT IX_DealHistory2 NONCLUSTERED (AccountId, AssetPairId)
);";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(IDeal).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(IDeal).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IDeal).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public DealsRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(DealsRepository), "CreateTableIfDoesntExists", null, ex);
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

        public async Task<IEnumerable<IDeal>> GetAsync(string accountId, string assetPairId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE 1=1 "
                    + (string.IsNullOrWhiteSpace(accountId) ? "" : " AccountId = @accountId")
                    + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AssetPairId = @assetPairId");
                
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
                    
                    _log?.WriteWarning(nameof(DealsRepository), nameof(AddAsync), msg);
                    
                    throw new Exception(msg);
                }
            }
        }
    }
}
