﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
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
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class OrdersHistorySqlRepository : IOrdersHistoryRepository
    {
        private const string TableName = "OrdersHistory";

        private static readonly string GetOrderBlotterPrimaryColumnsCommaSeparated =
            string.Join(",", typeof(IOrderHistoryForOrderBlotter).GetProperties().Select(x => x.Name));

        private readonly string _populateTpSlScript =
            $@"SELECT ParentOrderId, ExpectedOpenPrice, ModifiedTimestamp, Type  
FROM {TableName} WITH (NOLOCK) 
WHERE ParentOrderID IN @ids AND Type in ('TakeProfit', 'StopLoss','TrailingStop');";

        private readonly string _populateSpreadScript = $@"SELECT TOP 1 ExternalOrderId, Spread 
FROM dbo.ExecutionOrderBooks WITH (NOLOCK) 
WHERE ExternalOrderId IN @externalIds;";

        private readonly string _populateCommissionAndOnBehalfScript =
            $@"SELECT EventSourceId, ReasonType, SUM(ChangeAmount) AS Result 
FROM dbo.AccountHistory WITH (NOLOCK) 
WHERE ReasonType in ('Commission', 'OnBehalf') AND EventSourceId IN @ids
GROUP BY EventSourceId, ReasonType;";

        private string GetOrderBlotterScript(string whereClause, string paginationClause)
        {
            var filteredListScript = $@"
;WITH 
    filteredOrderList AS (
        SELECT *,
               CASE [Status]
                   WHEN 'Placed' THEN 0
                   WHEN 'Inactive'THEN 1
                   WHEN 'Active'THEN 2
                   WHEN 'ExecutionStarted'THEN 3
                   WHEN 'Executed'THEN 4
                   WHEN 'Canceled'THEN 5
                   WHEN 'Rejected'THEN 6
                   WHEN 'Expired'THEN 7
                   ELSE 99
               END as StatusOrder
        FROM {TableName} oh (NOLOCK)
        {whereClause}
    ),
    filteredOrderListWithRowNumber AS (
        SELECT *, ROW_NUMBER() OVER (PARTITION BY Id ORDER BY
            [ModifiedTimestamp] DESC,
            StatusOrder DESC
            ) AS RowNumber
        FROM filteredOrderList
    )";
            return $@"
{filteredListScript}
SELECT {GetOrderBlotterPrimaryColumnsCommaSeparated}
FROM filteredOrderListWithRowNumber WITH (NOLOCK)
WHERE RowNumber = 1
{paginationClause};
{filteredListScript}
SELECT COUNT(*)
FROM filteredOrderListWithRowNumber WITH (NOLOCK)
WHERE RowNumber = 1;";
        }

        private string GetAdditionalFieldsScript(string columnsCommaSeparated)
        {
            return $@"SELECT {columnsCommaSeparated} FROM history WITH (NOLOCK)
OUTER APPLY (
    SELECT
        TakeProfit = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS takeProfitHistory WITH (NOLOCK)
            WHERE takeProfitHistory.ParentOrderID = history.ID AND takeProfitHistory.Type = 'TakeProfit' and takeProfitHistory.ModifiedTimestamp >= history.ModifiedTimestamp
            ORDER BY ModifiedTimestamp ASC
            FOR JSON AUTO
        )
) AS TakeProfit
OUTER APPLY (
    SELECT
        StopLoss = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS stopLossHistory WITH (NOLOCK)
            WHERE stopLossHistory.ParentOrderID = history.ID AND stopLossHistory.Type in ('StopLoss','TrailingStop') and stopLossHistory.ModifiedTimestamp >= history.ModifiedTimestamp
            ORDER BY ModifiedTimestamp ASC
            FOR JSON AUTO
        )
) AS StopLoss
OUTER APPLY (
  SELECT
    Spread = (
      SELECT TOP 1 Spread
      FROM dbo.ExecutionOrderBooks AS eob WITH (NOLOCK)
      WHERE eob.ExternalOrderId = history.ExternalOrderId
    )
) AS Spread
OUTER APPLY (
  SELECT
    Commission = (
      SELECT SUM(ah.ChangeAmount)
      FROM dbo.AccountHistory AS ah WITH (NOLOCK)
      WHERE ah.ReasonType = 'Commission' AND ah.EventSourceId = history.Id
    )
) AS Commission
OUTER APPLY (
  SELECT
    OnBehalf = (
      SELECT SUM(ah.ChangeAmount)
      FROM dbo.AccountHistory AS ah WITH (NOLOCK)
      WHERE ah.ReasonType = 'OnBehalf' AND ah.EventSourceId = history.Id
    )
) AS OnBehalf";
        }

        private readonly string _connectionString;
        private readonly ILog _log;
        private readonly TimeSpan _orderBlotterExecutionTimeout;

        private static readonly string GetColumns =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IOrderHistory).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public OrdersHistorySqlRepository(string connectionString, ILog log, TimeSpan orderBlotterExecutionTimeout)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _orderBlotterExecutionTimeout = orderBlotterExecutionTimeout;

            connectionString.InitializeSqlObject("dbo.OrdersHistory.sql", log);
        }

        public Task AddAsync(IOrderHistory order, ITrade trade)
        {
            return TransactionWrapperExtensions.RunInTransactionAsync(
                (conn, transaction) => DoAdd(conn, transaction, order, trade),
                _connectionString,
                RollbackExceptionHandler,
                commitException => CommitExceptionHandler(commitException, order, trade));
        }

        public async Task<PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>> GetOrderBlotterAsync(
            DateTime relevanceTimestamp,
            string accountId,
            string assetPairId,
            string createdBy,
            List<OrderStatus> statuses,
            List<OrderType> orderTypes,
            List<OriginatorType> originatorTypes,
            DateTime? createdOnFrom,
            DateTime? createdOnTo,
            DateTime? modifiedOnFrom,
            DateTime? modifiedOnTo,
            int skip,
            int take)
        {
            var whereClause = $@"WHERE oh.ModifiedTimestamp <= @relevanceTimestamp
                {(string.IsNullOrEmpty(accountId) ? "" : " AND oh.AccountId = @accountId")}
                {(string.IsNullOrEmpty(assetPairId) ? "" : " AND oh.AssetPairId = @assetPairId")}
                {(string.IsNullOrEmpty(createdBy) ? "" : " AND oh.CreatedBy = @createdBy")}
                {(!(statuses?.Any() ?? false) ? "" : " AND oh.Status IN @statuses")}
                {(!(orderTypes?.Any() ?? false) ? "" : " AND oh.Type IN @orderTypes")}
                {(!(originatorTypes?.Any() ?? false) ? "" : " AND oh.Originator IN @originatorTypes")}
                {(!createdOnFrom.HasValue ? "" : " AND oh.CreatedTimestamp >= @createdOnFrom")}
                {(!createdOnTo.HasValue ? "" : " AND oh.CreatedTimestamp < @createdOnTo")}
                {(!modifiedOnFrom.HasValue ? "" : " AND oh.ModifiedTimestamp >= @modifiedOnFrom")}
                {(!modifiedOnTo.HasValue ? "" : " AND oh.ModifiedTimestamp < @modifiedOnTo")}";
            var paginationClause = "ORDER BY CreatedTimestamp DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    GetOrderBlotterScript(whereClause, paginationClause),
                    new
                    {
                        relevanceTimestamp,
                        accountId,
                        assetPairId,
                        createdBy,
                        statuses = statuses?.Select(x => x.ToString()).ToArray(),
                        orderTypes = orderTypes?.Select(x => x.ToString()).ToArray(),
                        originatorTypes = originatorTypes?.Select(x => x.ToString()).ToArray(),
                        createdOnFrom,
                        createdOnTo,
                        modifiedOnFrom,
                        modifiedOnTo,
                        skip,
                        take
                    },
                    commandTimeout: (int) _orderBlotterExecutionTimeout.TotalSeconds);
                var orderHistoryEntities = (await gridReader.ReadAsync<OrderHistoryForOrderBlotterEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                await PopulateAdditionalDataAsync(conn, orderHistoryEntities);

                return new PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>(
                    contents: orderHistoryEntities,
                    start: skip,
                    size: orderHistoryEntities.Count,
                    totalSize: totalCount
                );
            }
        }

        public async Task<IEnumerable<IOrderHistoryWithAdditional>> GetHistoryAsync(string orderId,
            OrderStatus? status = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var whereClause = "WHERE Id=@orderId"
                                  + (status == null ? "" : " AND Status=@status");
                var query =
                    $"WITH history AS (SELECT * FROM {TableName} WITH (NOLOCK) {whereClause}) {GetAdditionalFieldsScript("*")} ORDER BY [ModifiedTimestamp] DESC";
                var objects =
                    await conn.QueryAsync<OrderHistoryWithAdditionalEntity>(query,
                        new {orderId, status = status?.ToString(),});

                return objects;
            }
        }

        public async Task<PaginatedResponse<IOrderHistoryWithAdditional>> GetHistoryByPagesAsync(string accountId,
            string assetPairId,
            List<OrderStatus> statuses, List<OrderType> orderTypes, List<OriginatorType> originatorTypes,
            string parentOrderId = null,
            DateTime? createdTimeStart = null, DateTime? createdTimeEnd = null,
            DateTime? modifiedTimeStart = null, DateTime? modifiedTimeEnd = null,
            int? skip = null, int? take = null, bool isAscending = true,
            bool executedOrdersEssentialFieldsOnly = false)
        {
            var whereClause = " WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (statuses == null || statuses.Count == 0 ? "" : " AND Status IN @statuses")
                              + (orderTypes == null || orderTypes.Count == 0 ? "" : " AND [Type] IN @orderTypes")
                              + (originatorTypes == null || originatorTypes.Count == 0
                                  ? ""
                                  : " AND Originator IN @originatorTypes")
                              + (string.IsNullOrEmpty(parentOrderId) ? "" : " AND ParentOrderId = @parentOrderId")
                              + (createdTimeStart == null ? "" : " AND CreatedTimestamp >= @createdTimeStart")
                              + (createdTimeEnd == null ? "" : " AND CreatedTimestamp < @createdTimeEnd")
                              + (modifiedTimeStart == null ? "" : " AND ModifiedTimestamp >= @modifiedTimeStart")
                              + (modifiedTimeEnd == null ? "" : " AND ModifiedTimestamp < @modifiedTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause =
                $" ORDER BY [ModifiedTimestamp] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";

            using (var conn = new SqlConnection(_connectionString))
            {
                var additionalFieldsScript = GetAdditionalFieldsScript(
                    executedOrdersEssentialFieldsOnly
                        ? string.Join(",", OrderHistoryWithAdditionalEntity.ExecutedOrdersEssentialFieldsOnly)
                        : "*");
                var sql =
                    $"WITH history AS (SELECT * FROM {TableName} WITH (NOLOCK) {whereClause}) {additionalFieldsScript} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}";

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
                var orderHistoryEntities = (await gridReader.ReadAsync<OrderHistoryWithAdditionalEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<IOrderHistoryWithAdditional>(
                    contents: orderHistoryEntities,
                    start: skip ?? 0,
                    size: orderHistoryEntities.Count,
                    totalSize: totalCount
                );
            }
        }

        private async Task DoAdd(SqlConnection conn, SqlTransaction transaction, IOrderHistory order, ITrade trade)
        {
            var orderHistoryEntity = OrderHistoryEntity.Create(order);
            await conn.ExecuteAsync(
                $"insert into {TableName} ({GetColumns}) values ({GetFields})",
                orderHistoryEntity,
                transaction);

            if (trade != null)
            {
                var tradeEntity = TradeEntity.Create(trade);
                await conn.ExecuteAsync(
                    $"insert into {TradesSqlRepository.TableName} ({TradesSqlRepository.GetColumns}) values ({TradesSqlRepository.GetFields})",
                    tradeEntity,
                    transaction);
            }
        }

        private Task RollbackExceptionHandler(Exception exception)
        {
            var context =
                $"An attempt to rollback transaction failed due to the following exception: {exception.Message}";

            return _log.WriteErrorAsync(nameof(OrdersHistorySqlRepository), nameof(AddAsync), context, exception);
        }

        private Task CommitExceptionHandler(Exception exception, IOrderHistory order, ITrade trade)
        {
            var context = $"Error {exception.Message} \n" +
                          $"Entity <{nameof(IOrderHistory)}>: \n" +
                          order.ToJson() + " \n" +
                          $"Entity <{nameof(ITrade)}>: \n" +
                          trade?.ToJson();

            return _log.WriteErrorAsync(nameof(OrdersHistorySqlRepository), nameof(AddAsync), context, exception);
        }

        private async Task PopulateAdditionalDataAsync(SqlConnection conn,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var ids = orderHistoryEntities.Select(x => x.Id).ToList();
            var externalIds = orderHistoryEntities
                .Select(x => x.ExternalOrderId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (ids.Any())
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"{_populateTpSlScript} {_populateSpreadScript} {_populateCommissionAndOnBehalfScript}",
                    new
                    {
                        ids,
                        externalIds
                    },
                    commandTimeout: (int) _orderBlotterExecutionTimeout.TotalSeconds);

                await PopulateTakeProfitAndStopLossAsync(gridReader, orderHistoryEntities);
                await PopulateSpreadAsync(gridReader, orderHistoryEntities);
                await PopulateCommissionAndOnBehalfAsync(gridReader, orderHistoryEntities);
            }
        }

        private async Task PopulateTakeProfitAndStopLossAsync(SqlMapper.GridReader gridReader,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var tpSlList = (await gridReader.ReadAsync<OrderHistory>()).ToList();

            var tpList = tpSlList
                .Where(x => x.Type == OrderType.TakeProfit)
                .OrderByDescending(x => x.ModifiedTimestamp)
                .ToList();
            var slList = tpSlList
                .Where(x => x.Type == OrderType.StopLoss || x.Type == OrderType.TrailingStop)
                .OrderByDescending(x => x.ModifiedTimestamp)
                .ToList();

            orderHistoryEntities.ForEach(x =>
            {
                var tp = tpList.FirstOrDefault(l =>
                    l.ParentOrderId == x.Id && l.ModifiedTimestamp >= x.ModifiedTimestamp);
                if (tp != null)
                {
                    x.TakeProfitPrice = tp.ExpectedOpenPrice;
                }

                var sl = slList.FirstOrDefault(l =>
                    l.ParentOrderId == x.Id && l.ModifiedTimestamp >= x.ModifiedTimestamp);
                if (sl != null)
                {
                    x.TakeProfitPrice = sl.ExpectedOpenPrice;
                }
            });
        }

        private async Task PopulateCommissionAndOnBehalfAsync(SqlMapper.GridReader gridReader,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var list = (await gridReader.ReadAsync()).ToList();
            
            var commissionList = list.Where(x => x.ReasonType == "Commission").ToList();
            var onBehalfList = list.Where(x => x.ReasonType == "OnBehalf").ToList();

            orderHistoryEntities.ForEach(x =>
            {
                var commission = commissionList.FirstOrDefault(l => l.EventSourceId == x.Id);
                if (commission != null)
                {
                    x.Commission = commission.Result;
                }

                var onBehalf = onBehalfList.FirstOrDefault(l => l.EventSourceId == x.Id);
                if (onBehalf != null)
                {
                    x.OnBehalfFee = onBehalf.Result;
                }
            });
        }

        private async Task PopulateSpreadAsync(SqlMapper.GridReader gridReader,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var spreadList = (await gridReader.ReadAsync()).ToList();

            orderHistoryEntities.ForEach(x =>
            {
                var sp = spreadList.FirstOrDefault(l => l.ExternalOrderId == x.ExternalOrderId);
                if (sp != null)
                {
                    x.Spread = (decimal?)sp.Spread;
                }
            });
        }
    }
}
