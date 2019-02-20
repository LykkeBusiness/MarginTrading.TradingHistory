﻿using System;
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
    public class OrdersHistorySqlRepository : IOrdersHistoryRepository
    {
        private const string TableName = "OrdersHistory";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [bigint] NOT NULL IDENTITY (1,1),
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
[CancellationOriginator] [nvarchar] (64) NULL,
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
[AdditionalInfo] [nvarchar](MAX) NULL,
[CorrelationId] [nvarchar](64) NULL,
CONSTRAINT PK_{0}_OID PRIMARY KEY CLUSTERED (OID DESC),
INDEX IX_{0}_Base (Id, AccountId, AssetPairId, Status, ParentOrderId, ExecutedTimestamp, CreatedTimestamp, ModifiedTimestamp, Type, Originator)
);";

        private const string GetRelatedOrdersScript = "SELECT * FROM history " + 
@"OUTER APPLY (
    SELECT
        TakeProfit = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS takeProfitHistory
            WHERE takeProfitHistory.ParentOrderID = history.ID AND takeProfitHistory.Type = 'TakeProfit'
            ORDER BY ModifiedTimestamp DESC
            FOR JSON AUTO
        )
) AS TakeProfit
OUTER APPLY (
    SELECT
        StopLoss = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS stopLossHistory
            WHERE stopLossHistory.ParentOrderID = history.ID AND stopLossHistory.Type in ('StopLoss','TrailingStop')
            ORDER BY ModifiedTimestamp DESC
            FOR JSON AUTO
        )
) AS StopLoss";

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

        public async Task<IEnumerable<IOrderHistoryWithRelated>> GetHistoryAsync(string orderId,
            OrderStatus? status = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE Id=@orderId"
                              + (status == null ? "" : " AND Status=@status");
                var query =
                    $"WITH history AS (SELECT * FROM {TableName} {clause}) {GetRelatedOrdersScript} ORDER BY [ModifiedTimestamp] DESC";
                var objects = await conn.QueryAsync<OrderHistoryWithRelatedEntity>(query, new
                {
                    orderId,
                    status = status?.ToString(),
                });

                return objects;
            }
        }

        public async Task<PaginatedResponse<IOrderHistoryWithRelated>> GetHistoryByPagesAsync(string accountId, string assetPairId,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes,
            string parentOrderId = null,
            DateTime? createdTimeStart = null, DateTime? createdTimeEnd = null,
            DateTime? modifiedTimeStart = null, DateTime? modifiedTimeEnd = null,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            var whereClause = " WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (statuses == null || statuses.Count == 0 ? "" : " AND Status IN @statuses")
                              + (orderTypes == null || orderTypes.Count == 0 ? "" : " AND [Type] IN @orderTypes")
                              + (originatorTypes == null || originatorTypes.Count == 0 ? "" : " AND Originator IN @originatorTypes")
                              + (string.IsNullOrEmpty(parentOrderId) ? "" : " AND ParentOrderId = @parentOrderId")
                              + (createdTimeStart == null ? "": " AND CreatedTimestamp >= @createdTimeStart")
                              + (createdTimeEnd == null ? "": " AND CreatedTimestamp < @createdTimeEnd")
                              + (modifiedTimeStart == null ? "": " AND ModifiedTimestamp >= @modifiedTimeStart")
                              + (modifiedTimeEnd == null ? "": " AND ModifiedTimestamp < @modifiedTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [ModifiedTimestamp] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql =
                    $"WITH history AS (SELECT * FROM {TableName} {whereClause} {paginationClause}) {GetRelatedOrdersScript}; SELECT COUNT(*) FROM {TableName} {whereClause}";
                
                var gridReader = await conn.QueryMultipleAsync(
                    sql,
                    new
                    {
                        accountId, 
                        assetPairId, 
                        statuses = statuses?.Select(x => x.ToString()).ToArray(),
                        orderTypes = orderTypes?.Select(x => x.ToString()).ToArray(),
                        originatorTypes = originatorTypes?.Select(x => x.ToString()).ToArray(),
                        parentOrderId,
                        createdTimeStart,
                        createdTimeEnd,
                        modifiedTimeStart,
                        modifiedTimeEnd,
                    });
                var orderHistoryEntities = (await gridReader.ReadAsync<OrderHistoryWithRelatedEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
            
                return new PaginatedResponse<IOrderHistoryWithRelated>(
                    contents: orderHistoryEntities, 
                    start: skip ?? 0, 
                    size: orderHistoryEntities.Count, 
                    totalSize: totalCount
                );
            }
        }
    }
}
