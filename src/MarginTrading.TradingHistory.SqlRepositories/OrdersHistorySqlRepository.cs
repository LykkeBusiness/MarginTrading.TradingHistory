﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Dapper;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Extensions;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class OrdersHistorySqlRepository : IOrdersHistoryRepository
    {
        private const string TableName = "OrdersHistory";

        private static readonly string GetOrderBlotterPrimaryColumnsCommaSeparated =
            string.Join(",", typeof(IOrderHistoryForOrderBlotter).GetProperties().Select(x => x.Name));

        private readonly string _getAssetIdByNameScript =
            $@"SELECT Top 1 ProductId FROM dbo.Products WITH (NOLOCK) WHERE Name = @assetName";
        
        private readonly string _getAccountIdByNameScript =
            $@"SELECT Top 1 Id FROM dbo.MarginTradingAccounts WITH (NOLOCK) WHERE AccountName = @accountName";

        private readonly string _populateTpSlScript =
            $@"SELECT ParentOrderId, ExpectedOpenPrice, ModifiedTimestamp, Type  
FROM {TableName} WITH (NOLOCK) 
WHERE ParentOrderID IN @ids AND Type in ('TakeProfit', 'StopLoss','TrailingStop');";

        private readonly string _populateSpreadScript = $@"SELECT ExternalOrderId, Spread 
FROM dbo.ExecutionOrderBooks WITH (NOLOCK) 
WHERE ExternalOrderId IN @externalIds;";

        private readonly string _populateCommissionAndOnBehalfScript =
            $@"SELECT EventSourceId, ReasonType, SUM(ChangeAmount) AS Result 
FROM dbo.AccountHistory WITH (NOLOCK) 
WHERE ReasonType in ('Commission', 'OnBehalf') AND EventSourceId IN @ids
GROUP BY EventSourceId, ReasonType;";
        
        private readonly string _populateAssetNameScript =
            $@"SELECT ProductId, Name  
FROM dbo.Products WITH (NOLOCK) 
WHERE ProductId IN @assetIds";
        
        private readonly string _populateAccountNameScript =
            $@"SELECT Id, AccountName  
FROM dbo.MarginTradingAccounts WITH (NOLOCK) 
WHERE Id IN @accountIds";

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
        private readonly ILogger _logger;
        private readonly TimeSpan? _orderBlotterExecutionTimeout;

        private static readonly string GetColumns =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(OrderHistoryEntity).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IOrderHistory).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public OrdersHistorySqlRepository(string connectionString, ILogger<OrdersHistorySqlRepository> logger, TimeSpan? orderBlotterExecutionTimeout = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderBlotterExecutionTimeout = orderBlotterExecutionTimeout;

            connectionString.InitializeSqlObject("dbo.OrdersHistory.sql", logger: logger);
        }

        public Task AddAsync(IOrderHistory order, ITrade trade)
        {
            return TransactionWrapperExtensions.RunInTransactionAsync(
                (conn, transaction) => DoAdd(conn, transaction, order, trade),
                _connectionString,
                RollbackExceptionHandler,
                commitException => CommitExceptionHandler(commitException, order, trade));
        }

        private string ToOrderHistoryColumn(OrderBlotterSortingColumn sortingColumn)
        {
            switch (sortingColumn)
            {
                case OrderBlotterSortingColumn.Price:
                    return nameof(IOrderHistoryForOrderBlotter.ExecutionPrice);
                case OrderBlotterSortingColumn.Quantity:
                    return nameof(IOrderHistoryForOrderBlotter.Volume);
                case OrderBlotterSortingColumn.Validity:
                    return nameof(IOrderHistoryForOrderBlotter.ValidityTime);
                case OrderBlotterSortingColumn.CreatedOn:
                    return nameof(IOrderHistoryForOrderBlotter.CreatedTimestamp);
                case OrderBlotterSortingColumn.ExchangeRate:
                    return nameof(IOrderHistoryForOrderBlotter.FxRate);
                case OrderBlotterSortingColumn.ModifiedOn:
                    return nameof(IOrderHistoryForOrderBlotter.ModifiedTimestamp);
                default: throw new NotImplementedException();
            }
        }

        public async Task<IEnumerable<string>> GetCreatedByOnBehalfListAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT DISTINCT CreatedBy FROM {TableName} where Originator = 'OnBehalf'";
                var result = (await conn.QueryAsync<string>(query))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .OrderBy(x => x)
                    .ToList();
                return result;
            }
        }

        public async Task<PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>> GetOrderBlotterAsync(
            DateTime relevanceTimestamp,
            string accountIdOrName,
            string assetName,
            string createdBy,
            List<OrderStatus> statuses,
            List<OrderType> orderTypes,
            List<OriginatorType> originatorTypes,
            DateTime? createdOnFrom,
            DateTime? createdOnTo,
            DateTime? modifiedOnFrom,
            DateTime? modifiedOnTo,
            int skip,
            int take,
            OrderBlotterSortingColumn sortingColumn,
            SortingOrder sortingOrder)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            using (var conn = new SqlConnection(_connectionString))
            {
                var assetPairId = (string)null;
                if (!string.IsNullOrWhiteSpace(assetName))
                {
                    assetPairId = await conn.QueryFirstOrDefaultAsync<string>(_getAssetIdByNameScript, new {assetName});
                    if (assetPairId == null)
                    {
                        return new PaginatedResponse<IOrderHistoryForOrderBlotterWithAdditionalData>(
                            contents: new List<IOrderHistoryForOrderBlotterWithAdditionalData>(),
                            start: skip,
                            size: 0,
                            totalSize: 0
                        );
                    }
                }
                
                var accountIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(accountIdOrName))
                {
                    accountIds.Add(accountIdOrName);
                    var accountId = await conn.QueryFirstOrDefaultAsync<string>(_getAccountIdByNameScript, new
                    {
                        accountName = accountIdOrName
                    });
                    if (!string.IsNullOrWhiteSpace(accountId))
                    {
                        accountIds.Add(accountId);
                    }
                }
                
                var whereClause = $@"WHERE oh.ModifiedTimestamp <= @relevanceTimestamp
                    {(!accountIds.Any() ? "" : " AND oh.AccountId IN @accountIds")}
                    {(string.IsNullOrEmpty(assetPairId) ? "" : " AND oh.AssetPairId = @assetPairId")}
                    {(string.IsNullOrEmpty(createdBy) ? "" : " AND oh.CreatedBy = @createdBy")}
                    {(!(statuses?.Any() ?? false) ? "" : " AND oh.Status IN @statuses")}
                    {(!(orderTypes?.Any() ?? false) ? "" : " AND oh.Type IN @orderTypes")}
                    {(!(originatorTypes?.Any() ?? false) ? "" : " AND oh.Originator IN @originatorTypes")}
                    {(!createdOnFrom.HasValue ? "" : " AND oh.CreatedTimestamp >= @createdOnFrom")}
                    {(!createdOnTo.HasValue ? "" : " AND oh.CreatedTimestamp < @createdOnTo")}
                    {(!modifiedOnFrom.HasValue ? "" : " AND oh.ModifiedTimestamp >= @modifiedOnFrom")}
                    {(!modifiedOnTo.HasValue ? "" : " AND oh.ModifiedTimestamp < @modifiedOnTo")}";
                var paginationClause = $"ORDER BY {ToOrderHistoryColumn(sortingColumn)} {(sortingOrder == SortingOrder.ASC ? "ASC" : "DESC")} OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
                
                var gridReader = await conn.QueryMultipleAsync(
                    GetOrderBlotterScript(whereClause, paginationClause),
                    new
                    {
                        relevanceTimestamp,
                        accountIds,
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
                    commandTimeout: (int) _orderBlotterExecutionTimeout?.TotalSeconds);
                var orderHistoryEntities = (await gridReader.ReadAsync<OrderHistoryForOrderBlotterEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                var batches = orderHistoryEntities.Batch(500); // sql limit is 2100, subqueries use up to 4*500 = 2000 parameters

                await PopulateAdditionalDataAsync(batches);

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

            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

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
                $" ORDER BY [ModifiedTimestamp] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {take} ROWS ONLY";

            using (var conn = new SqlConnection(_connectionString))
            {
                var additionalFieldsScript = GetAdditionalFieldsScript(
                    executedOrdersEssentialFieldsOnly
                        ? string.Join(",", OrderHistoryWithAdditionalEntity.ExecutedOrdersEssentialFieldsOnly)
                        : "*");
                var sql =
                    $"WITH history AS (SELECT * FROM {TableName} WITH (NOLOCK) {whereClause}) {additionalFieldsScript} {paginationClause}; SELECT COUNT(*) FROM {TableName} WITH (NOLOCK) {whereClause}";

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
                transaction,
                true,
                logger: _logger);

            if (trade != null)
            {
                var tradeEntity = TradeEntity.Create(trade);
                await conn.ExecuteAsync(
                    $"insert into {TradesSqlRepository.TableName} ({TradesSqlRepository.GetColumns}) values ({TradesSqlRepository.GetFields})",
                    tradeEntity,
                    transaction,
                    true,
                    logger: _logger);
            }
        }

        private Task RollbackExceptionHandler(Exception exception)
        {
            var context =
                $"An attempt to rollback transaction failed due to the following exception: {exception.Message}";

            _logger.LogError(exception, context);
            return Task.CompletedTask;
        }

        private Task CommitExceptionHandler(Exception exception, IOrderHistory order, ITrade trade)
        {
            var context = $"Error {exception.Message} \n" +
                          $"Entity <{nameof(IOrderHistory)}>: \n" +
                          order.ToJson() + " \n" +
                          $"Entity <{nameof(ITrade)}>: \n" +
                          trade?.ToJson();

            _logger.LogError(exception, context);
            return Task.CompletedTask;
        }

        private async Task PopulateAdditionalDataAsync(IEnumerable<IEnumerable<OrderHistoryForOrderBlotterEntity>> batches)
        {
            var allTasks = new List<Task>();
            var maxThreads = 4;
            var throttler = new SemaphoreSlim(initialCount: maxThreads);
            foreach (var batch in batches)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            using var connection = new SqlConnection(_connectionString);
                            await PopulateAdditionalDataAsync(connection, batch.ToList());
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }
            await Task.WhenAll(allTasks);
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
            var assetIds = orderHistoryEntities
                .Select(x => x.AssetPairId)
                .Distinct()
                .ToList();
            var accountIds = orderHistoryEntities
                .Select(x => x.AccountId)
                .Distinct()
                .ToList();

            if (ids.Any())
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"{_populateTpSlScript} {_populateSpreadScript} {_populateCommissionAndOnBehalfScript} {_populateAssetNameScript} {_populateAccountNameScript}",
                    new
                    {
                        ids,
                        externalIds,
                        assetIds,
                        accountIds
                    },
                    commandTimeout: (int) _orderBlotterExecutionTimeout?.TotalSeconds);

                await PopulateTakeProfitAndStopLossAsync(gridReader, orderHistoryEntities);
                await PopulateSpreadAsync(gridReader, orderHistoryEntities);
                await PopulateCommissionAndOnBehalfAsync(gridReader, orderHistoryEntities);
                await PopulateAssetNameAsync(gridReader, orderHistoryEntities);
                await PopulateAccountNameAsync(gridReader, orderHistoryEntities);
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
                    x.StopLossPrice = sl.ExpectedOpenPrice;
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

        private async Task PopulateAssetNameAsync(SqlMapper.GridReader gridReader,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var productList = (await gridReader.ReadAsync()).ToList();

            orderHistoryEntities.ForEach(x =>
            {
                var product = productList.FirstOrDefault(l => l.ProductId == x.AssetPairId);
                if (product != null)
                {
                    x.AssetName = product.Name;
                }
            });
        }

        private async Task PopulateAccountNameAsync(SqlMapper.GridReader gridReader,
            List<OrderHistoryForOrderBlotterEntity> orderHistoryEntities)
        {
            var accountList = (await gridReader.ReadAsync()).ToList();

            orderHistoryEntities.ForEach(x =>
            {
                var account = accountList.FirstOrDefault(l => l.Id == x.AccountId);
                if (account != null)
                {
                    x.AccountName = account.AccountName;
                }
            });
        }
    }
}
