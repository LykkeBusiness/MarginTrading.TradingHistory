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
    public class PositionsHistorySqlRepository : IPositionsHistoryRepository
    {
        private const string TableName = "PositionsHistory";

        private const string CreateTableScript = @"CREATE TABLE [{0}](
[OID] [bigint] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[Id] [nvarchar](64) NOT NULL,
[DealId] [nvarchar](128) NULL,
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
[OpenOrderType] [nvarchar] (64) NULL,
[OpenOrderVolume] [float] NULL,
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
[FxAssetPairId] [nvarchar] (64) NULL,
[FxToAssetPairDirection] [nvarchar] (64) NULL,
[LastModified] [datetime] NULL,
[TotalPnL] [float] NULL,
[ChargedPnl] [float] NULL,
[AdditionalInfo] [nvarchar] (MAX) NULL,
[HistoryType] [nvarchar] (64) NULL,
[DealInfo] [nvarchar] (1024) NULL,
[HistoryTimestamp] [datetime] NULL,
INDEX IX_{0}_Base (Id, AccountId, AssetPairId)
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
                    _log?.WriteErrorAsync("PositionHistory", "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(IPositionHistory positionHistory, IDeal deal)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var transaction = conn.BeginTransaction();

                try
                {
                    var positionEntity = PositionsHistoryEntity.Create(positionHistory);
                    await conn.ExecuteAsync($"insert into {TableName} ({GetColumns}) values ({GetFields})",
                        positionEntity,
                        transaction);

                    if (deal != null)
                    {
                        var dealEntity = DealEntity.Create(deal);
                        await conn.ExecuteAsync(
                            $"insert into {DealsSqlRepository.TableName} ({DealsSqlRepository.GetColumns}) values ({DealsSqlRepository.GetFields})",
                            dealEntity,
                            transaction);
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
                    
                    _log?.WriteWarning("PositionsHistorySqlRepository", "AddAsync", msg);
                    
                    throw new Exception(msg);
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
