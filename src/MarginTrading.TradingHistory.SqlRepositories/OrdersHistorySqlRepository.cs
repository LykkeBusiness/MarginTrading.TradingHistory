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
    public class OrdersHistorySqlRepository : IOrdersHistoryRepository
    {
        private const string TableName = "OrdersHistory";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[Id] [nvarchar](64) NOT NULL,
[Code] [bigint] NULL,
[AccountId] [nvarchar] (64) NULL,
[AssetPairId] [nvarchar] (64) NULL,
[ParentOrderId] [nvarchar] (64) NULL,
[PositionId] [nvarchar] (64) NULL,
[Direction] [nvarchar] (64) NULL,
[Type] [nvarchar] (64) NULL,
[Status] [nvarchar] (64) NULL,
[Originator] [nvarchar] (64) NULL,
[Volume] [float] NULL,
[ExpectedOpenPrice] [float] NULL,
[ExecutionPrice] [float] NULL,
[FxRate] [float] NULL,
[ForceOpen] [bit] NULL,
[ValidityTime] [datetime] NULL,
[CreatedTimestamp] [datetime] NULL,
[ModifiedTimestamp] [datetime] NULL,
[ActivatedTimestamp] [datetime] NULL,
[ExecutionStartedTimestamp] [datetime] NULL,
[ExecutedTimestamp] [datetime] NULL,
[CanceledTimestamp] [datetime] NULL,
[Rejected] [datetime] NULL,
[TradingConditionId] [nvarchar] (64) NULL,
[AccountAssetId] [nvarchar] (64) NULL,
[EquivalentAsset] [nvarchar] (64) NULL,
[EquivalentRate] [float] NULL,
[RejectReason] [nvarchar] (64) NULL,
[RejectReasonText] [nvarchar] (1024) NULL,
[Comment] [nvarchar] (1024) NULL,
[ExternalOrderId] [nvarchar] (64) NULL,
[ExternalProviderId] [nvarchar] (64) NULL,
[MatchingEngineId] [nvarchar] (64) NULL,
[LegalEntity] [nvarchar] (64) NULL,
[UpdateType] [nvarchar] (64) NULL,
[MatchedOrders] [nvarchar](MAX) NULL,
[RelatedOrderInfos] [nvarchar](MAX) NULL,
[AdditionalInfo] [nvarchar](MAX) NULL
);";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IOrderHistory).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public OrdersHistorySqlRepository(string connectionString, ILog log)
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

        public async Task AddAsync(IOrderHistory order)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    var entity = OrderHistoryEntity.Create(order);
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              "Entity <IOrderHistory>: \n" +
                              order.ToJson();
                    
                    _log?.WriteWarning("AccountTransactionsReportsSqlRepository", "InsertOrReplaceAsync", msg);
                    
                    throw new Exception(msg);
                }
            }
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName}";
                var objects = await conn.QueryAsync<OrderHistoryEntity>(query);
                
                return objects;
            }
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(string accountId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} Where AccountId = @accountId";
                var objects = await conn.QueryAsync<OrderHistoryEntity>(query, new {accountId});
                
                return objects;
            }
        }

        public async Task<IEnumerable<IOrderHistory>> GetHistoryAsync(Func<IOrderHistory, bool> predicate)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName}";
                var objects = await conn.QueryAsync<OrderHistoryEntity>(query);

                return objects.Where(predicate);
            }
        }
    }
}
