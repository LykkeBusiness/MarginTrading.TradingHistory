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
    public class OrdersHistorySqlRepository : IOrdersHistoryRepository
    {
        private const string TableName = "OrdersHistory";

        private string GetAdditionalFieldsScript(string columnsCommaSeparated)
        {
            return $@"SELECT {columnsCommaSeparated} FROM history WITH (NOLOCK)
OUTER APPLY (
    SELECT
        TakeProfit = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS takeProfitHistory WITH (NOLOCK)
            WHERE takeProfitHistory.ParentOrderID = history.ID AND takeProfitHistory.Type = 'TakeProfit'
            ORDER BY ModifiedTimestamp DESC
            FOR JSON AUTO
        )
) AS TakeProfit
OUTER APPLY (
    SELECT
        StopLoss = (
            SELECT TOP 1 Id, Type, ExpectedOpenPrice, Status, ModifiedTimestamp
            FROM OrdersHistory AS stopLossHistory WITH (NOLOCK)
            WHERE stopLossHistory.ParentOrderID = history.ID AND stopLossHistory.Type in ('StopLoss','TrailingStop')
            ORDER BY ModifiedTimestamp DESC
            FOR JSON AUTO
        )
) AS StopLoss
OUTER APPLY (
  SELECT
    Spread = (
      SELECT TOP 1 Spread
      FROM dbo.ExecutionOrderBooks AS eob WITH (NOLOCK)
      WHERE eob.OrderId = history.Id
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

            connectionString.InitializeSqlObject("dbo.OrdersHistory.sql", log);
        }

        public async Task AddAsync(IOrderHistory order, ITrade trade)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var transaction = conn.BeginTransaction();

                try
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

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var msg = $"Error {ex.Message} \n" +
                              $"Entity <{nameof(IOrderHistory)}>: \n" +
                              order.ToJson() + " \n" +
                              $"Entity <{nameof(ITrade)}>: \n" +
                              trade?.ToJson();

                    _log?.WriteError(nameof(OrdersHistorySqlRepository), nameof(AddAsync),
                        new Exception(msg));

                    throw;
                }
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
                var objects = await conn.QueryAsync<OrderHistoryWithAdditionalEntity>(query, new
                {
                    orderId,
                    status = status?.ToString(),
                });

                return objects;
            }
        }

        public async Task<PaginatedResponse<IOrderHistoryWithAdditional>> GetHistoryByPagesAsync(string accountId, string assetPairId,
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
                              + (originatorTypes == null || originatorTypes.Count == 0 ? "" : " AND Originator IN @originatorTypes")
                              + (string.IsNullOrEmpty(parentOrderId) ? "" : " AND ParentOrderId = @parentOrderId")
                              + (createdTimeStart == null ? "" : " AND CreatedTimestamp >= @createdTimeStart")
                              + (createdTimeEnd == null ? "" : " AND CreatedTimestamp < @createdTimeEnd")
                              + (modifiedTimeStart == null ? "" : " AND ModifiedTimestamp >= @modifiedTimeStart")
                              + (modifiedTimeEnd == null ? "" : " AND ModifiedTimestamp < @modifiedTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [ModifiedTimestamp] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";

            using (var conn = new SqlConnection(_connectionString))
            {
                var additionalFieldsScript = GetAdditionalFieldsScript(
                    executedOrdersEssentialFieldsOnly
                        ? string.Join(",", OrderHistoryEntity.ExecutedOrdersEssentialFieldsOnly)
                        : "*");
                var sql =
                    $"WITH history AS (SELECT * FROM {TableName} WITH (NOLOCK) {whereClause} {paginationClause}) {additionalFieldsScript}; SELECT COUNT(*) FROM {TableName} {whereClause}";

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
    }
}
