// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
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
    public class PositionsHistorySqlRepository : IPositionsHistoryRepository
    {
        private const string TableName = "PositionsHistory";

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(PositionsHistoryEntity).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(PositionsHistoryEntity).GetProperties().Select(x => "@" + x.Name));

        public PositionsHistorySqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            
            connectionString.InitializeSqlObject("dbo.Deals.sql", log);
            connectionString.InitializeSqlObject("dbo.DealCommissionParams.sql", log);
            connectionString.InitializeSqlObject("dbo.UpdateDealCommissionParamsOnDeal.sql", log);
            connectionString.InitializeSqlObject("dbo.PositionHistory.sql", log);
        }

        public async Task AddAsync(IPositionHistory positionHistory, IDeal deal)
        {
            await TransactionWrapperExtensions.RunInTransactionAsync(
                (conn, transaction) => DoAdd(conn, transaction, positionHistory, deal),
                _connectionString,
                RollbackExceptionHandler,
                commitException => CommitExceptionHandler(commitException, positionHistory, deal));

            if (deal != null)
            {
                await Task.Run(async () =>
                {
                    using var conn = new SqlConnection(_connectionString);
                    try
                    {
                        await conn.ExecuteAsync("[dbo].[UpdateDealCommissionParamsOnDeal]",
                            new
                            {
                                deal.DealId,
                                deal.OpenTradeId,
                                deal.OpenOrderVolume,
                                deal.CloseTradeId,
                                deal.CloseOrderVolume,
                                deal.Volume,
                            },
                            commandType: CommandType.StoredProcedure,
                            ignoreDuplicates: true,
                            log: _log);
                    }
                    catch (Exception e)
                    {
                        await _log.WriteErrorAsync(nameof(PositionsHistorySqlRepository),
                            nameof(AddAsync),
                            $"Failed to calculate commissions for the deal {deal.DealId}, skipping.", e);
                    }
                });

            }
        }

        public async Task<List<IPositionHistory>> GetAsync(string accountId,
            string assetPairId,
            DateTime? eventDateFrom,
            DateTime? eventDateTo)
        {
            using var conn = new SqlConnection(_connectionString);
            var whereClause = "Where 1=1 " +
                              (string.IsNullOrEmpty(accountId) ? "" : " And AccountId = @accountId") +
                              (string.IsNullOrEmpty(assetPairId) ? "" : " And AssetPairId = @assetPairId") +
                              (eventDateFrom == null ? "" : " AND CONVERT(date, HistoryTimestamp) >= @eventDateFrom") +
                              (eventDateTo == null ? "" : " AND CONVERT(date, HistoryTimestamp) <= @eventDateTo");

            var query = $"SELECT * FROM {TableName} {whereClause}";
            var objects =
                await conn.QueryAsync<PositionsHistoryEntity>(query, new { accountId, assetPairId, eventDateFrom, eventDateTo });

            return objects.Cast<IPositionHistory>().ToList();
        }

        public async Task<PaginatedResponse<IPositionHistory>> GetByPagesAsync(string accountId,
            string assetPairId,
            DateTime? eventDateFrom,
            DateTime? eventDateTo,
            int? skip = null,
            int? take = null)
        {
            var whereClause = " WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (eventDateFrom == null ? "" : " AND CONVERT(date, HistoryTimestamp) >= @eventDateFrom")
                              + (eventDateTo == null ? "" : " AND CONVERT(date, HistoryTimestamp) <= @eventDateTo");

            using var conn = new SqlConnection(_connectionString);
            var paginationClause =
                $" ORDER BY [Oid] OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
            var gridReader = await conn.QueryMultipleAsync(
                $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                new { accountId, assetPairId, eventDateFrom, eventDateTo });
            var positionsHistoryEntities = (await gridReader.ReadAsync<PositionsHistoryEntity>()).ToList();
            var totalCount = await gridReader.ReadSingleAsync<int>();

            return new PaginatedResponse<IPositionHistory>(
                contents: positionsHistoryEntities,
                start: skip ?? 0,
                size: positionsHistoryEntities.Count,
                totalSize: totalCount
            );
        }

        public async Task<List<IPositionHistory>> GetAsync(string id)
        {
            using var conn = new SqlConnection(_connectionString);
            var query = $"SELECT * FROM {TableName} Where Id = @id";
            var objects = await conn.QueryAsync<PositionsHistoryEntity>(query, new {id});
                
            return objects.Cast<IPositionHistory>().ToList();
        }
        
        private async Task DoAdd(SqlConnection conn, SqlTransaction transaction, IPositionHistory positionHistory, IDeal deal)
        {
            var positionEntity = PositionsHistoryEntity.Create(positionHistory);

            await conn.ExecuteAsync($"insert into {TableName} ({GetColumns}) values ({GetFields})",
                positionEntity,
                transaction,
                true,
                log: _log);

            if (deal != null)
            {
                var entity = DealEntity.Create(deal);
                
                await conn.ExecuteAsync(
                    $@"INSERT INTO [dbo].[Deals] ({string.Join(",", DealsSqlRepository.DealInsertColumns)}) VALUES (@{string.Join(",@", DealsSqlRepository.DealInsertColumns)})",
                    new
                    {
                        entity.DealId,
                        entity.Created,
                        entity.AccountId,
                        entity.AssetPairId,
                        entity.OpenTradeId,
                        entity.OpenOrderType,
                        entity.OpenOrderVolume,
                        entity.OpenOrderExpectedPrice,
                        entity.CloseTradeId,
                        entity.CloseOrderType,
                        entity.CloseOrderVolume,
                        entity.CloseOrderExpectedPrice,
                        entity.Direction,
                        entity.Volume,
                        entity.Originator,
                        entity.OpenPrice,
                        entity.OpenFxPrice,
                        entity.ClosePrice,
                        entity.CloseFxPrice,
                        entity.Fpl,
                        entity.PnlOfTheLastDay,
                        entity.AdditionalInfo,
                        entity.CorrelationId
                    },
                    transaction, 
                    true,
                    log: _log);

                await conn.ExecuteAsync("INSERT INTO [dbo].[DealCommissionParams] (DealId) VALUES (@DealId)",
                    new {deal.DealId},
                    transaction,
                    true,
                    log: _log);
            }
        }

        private Task RollbackExceptionHandler(Exception exception)
        {
            var context =
                $"An attempt to rollback transaction failed due to the following exception: {exception.Message}";

            return _log.WriteErrorAsync(nameof(PositionsHistorySqlRepository), nameof(AddAsync), context, exception);
        }

        private Task CommitExceptionHandler(Exception exception, IPositionHistory positionHistory, IDeal deal)
        {
            var context = $"Error {exception.Message} \n" +
                      $"Entity <{nameof(IPositionHistory)}>: \n" +
                      positionHistory.ToJson() + " \n" +
                      $"Entity <{nameof(IDeal)}>: \n" +
                      deal?.ToJson();

            return _log.WriteErrorAsync(nameof(PositionsHistorySqlRepository), nameof(AddAsync), context, exception);
        }
    }
}
