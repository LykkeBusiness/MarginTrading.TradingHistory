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

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class PositionsHistorySqlRepository : IPositionsHistoryRepository
    {
        private const string TableName = "PositionsHistory";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[Id] [nvarchar](64) NOT NULL,
[Code] [bigint] NULL,
[AssetPairId] [nvarchar] (64) NULL,
[Direction] [nvarchar] (64) NULL,
[Volume] [float] NULL,
[AccountId] [nvarchar] (64) NULL,
[TradingConditionId] [nvarchar] (64) NULL,
[AccountAssetId] [nvarchar] (64) NULL,
[ExpectedOpenPrice] [float] NULL,
[OpenMatchingEngineId] [nvarchar] (64) NULL,
[OpenDate] [datetime] NULL,
[OpenTradeId] [nvarchar] (64) NULL,
[OpenPrice] [float] NULL,
[OpenFxPrice] [float] NULL,
[EquivalentAsset] [nvarchar] (64) NULL,
[OpenPriceEquivalent] [float] NULL,
[RelatedOrders] [nvarchar](1024) NULL,
[LegalEntity] [nvarchar] (64) NULL,
[OpenOriginator] [nvarchar] (64) NULL,
[ExternalProviderId] [nvarchar] (64) NULL,
[SwapCommissionRate] [float] NULL,
[OpenCommissionRate] [float] NULL,
[CloseCommissionRate] [float] NULL,
[CommissionLot] [float] NULL,
[CloseMatchingEngineId] [nvarchar] (64) NULL,
[ClosePrice] [float] NULL,
[CloseFxPrice] [float] NULL,
[ClosePriceEquivalent] [float] NULL,
[StartClosingDate] [datetime] NULL,
[CloseDate] [datetime] NULL,
[CloseOriginator] [nvarchar] (64) NULL,
[CloseReason] [nvarchar] (256) NULL,
[CloseComment] [nvarchar] (256) NULL,
[CloseTrades] [nvarchar] (1024) NULL,
[LastModified] [datetime] NULL,
[TotalPnL] [float] NULL,
[HistoryType] [nvarchar] (64) NULL,
[DealInfo] [nvarchar] (1024) NULL,
[HistoryTimestamp] [datetime] NULL
);";

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
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync("OrdersChangeHistory", "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(IPositionHistory positionHistory)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    var entity = PositionsHistoryEntity.Create(positionHistory);
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              "Entity <IOrderHistory>: \n" +
                              positionHistory.ToJson();
                    
                    _log?.WriteWarning("PositionsHistorySqlRepository", "AddAsync", msg);
                    
                    throw new Exception(msg);
                }
            }
        }

        public async Task<IEnumerable<IPositionHistory>> GetAsync(string accountId, string assetPairId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var whereClause = "Where HistoryType = 'Close'" +
                                  (string.IsNullOrEmpty(accountId) ? "" : " And AccountId = @accountId") +
                                  (string.IsNullOrEmpty(assetPairId) ? "" : " And AssetPairId = @assetPairId");

                var query = $"SELECT * FROM {TableName} {whereClause}";
                var objects = await conn.QueryAsync<PositionsHistoryEntity>(query, new {accountId, assetPairId});
                
                return objects;
            }
        }

        public async Task<IPositionHistory> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} Where Id = @id And HistoryType = 'Close'";
                var objects = await conn.QueryAsync<PositionsHistoryEntity>(query, new {id});
                
                return objects.SingleOrDefault();
            }
        }
    }
}
