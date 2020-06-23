// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using Lykke.Common.Log;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class DealsSqlRepository : IDealsRepository
    {
        private const string ViewName = "[dbo].[V_DealsWithCommissionParams]";
        private const string TableName = "[dbo].[Deals]";
        private readonly ILog _log;

        private readonly string _connectionString;

        public static readonly List<string> DealInsertColumns = typeof(IDeal).GetProperties().Select(x => x.Name).ToList();

        public DealsSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;

            connectionString.InitializeSqlObject("dbo.Deals.sql", log);
            connectionString.InitializeSqlObject("dbo.DealCommissionParams.sql", log);
            connectionString.InitializeSqlObject("dbo.V_DealsWithCommissionParams.sql", log);

            _log = log;
        }
        
        public async Task<IDealWithCommissionParams> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {ViewName} WHERE DealId = @id";
                var objects = await conn.QueryAsync<DealWithCommissionParamsEntity>(query, new {id});
                return objects.FirstOrDefault();
            }
        }

        public async Task<PaginatedResponse<IDealWithCommissionParams>> GetByPagesAsync(string accountId,
            string assetPairId,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                              + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [Created] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {ViewName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {ViewName} {whereClause}",
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
                var deals = (await gridReader.ReadAsync<DealWithCommissionParamsEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
            
                return new PaginatedResponse<IDealWithCommissionParams>(
                    contents: deals, 
                    start: skip ?? 0, 
                    size: deals.Count, 
                    totalSize: totalCount
                );
            }
        }

        public async Task<PaginatedResponse<IAggregatedDeal>> GetAggregated(string accountId, string assetPairId,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            var whereClause = "WHERE AccountId=@accountId"
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                              + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [{nameof(IAggregatedDeal.LastDealDate)}] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";

            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $@"SELECT {nameof(IAggregatedDeal.AccountId)},
                        {nameof(IAggregatedDeal.AssetPairId)},
                        SUM({nameof(IDeal.Volume)}) AS {nameof(IAggregatedDeal.Volume)},
                        SUM({nameof(IDeal.Fpl)}) AS {nameof(IAggregatedDeal.Fpl)},
                        SUM(ROUND({nameof(IDeal.Fpl)} / {nameof(IDeal.CloseFxPrice)}, 2)) AS {nameof(IAggregatedDeal.FplTc)},
                        SUM({nameof(IDeal.PnlOfTheLastDay)}) AS {nameof(IAggregatedDeal.PnlOfTheLastDay)},
                        SUM({nameof(IDealWithCommissionParams.OvernightFees)}) AS {nameof(IAggregatedDeal.OvernightFees)},
                        SUM({nameof(IDealWithCommissionParams.Commission)}) AS {nameof(IAggregatedDeal.Commission)},
                        SUM({nameof(IDealWithCommissionParams.OnBehalfFee)}) AS {nameof(IAggregatedDeal.OnBehalfFee)},
                        SUM({nameof(IDealWithCommissionParams.Taxes)}) AS {nameof(IAggregatedDeal.Taxes)},
                        COUNT({nameof(IDeal.DealId)}) AS {nameof(IAggregatedDeal.DealsCount)},
                        MAX({nameof(IDeal.Created)}) AS {nameof(IAggregatedDeal.LastDealDate)}
                      FROM {ViewName}
                      {whereClause}
                      GROUP BY {nameof(IAggregatedDeal.AccountId)}, {nameof(IAggregatedDeal.AssetPairId)}
                      {paginationClause}; SELECT COUNT(DISTINCT {nameof(IAggregatedDeal.AssetPairId)}) FROM {ViewName} {whereClause}",
                    new { accountId, assetPairId, closeTimeStart, closeTimeEnd });
                var deals = (await gridReader.ReadAsync<AggregatedDeal>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<IAggregatedDeal>(
                    contents: deals,
                    start: skip ?? 0,
                    size: deals.Count,
                    totalSize: totalCount
                );
            }
        }

        public async Task<IEnumerable<IDealWithCommissionParams>> GetAsync(string accountId, 
            string assetPairId, DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE 1=1 "
                    + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                    + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId = @assetPairId")
                    + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                    + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
                
                var query = $"SELECT * FROM {ViewName} {clause}";
                return await conn.QueryAsync<DealWithCommissionParamsEntity>(query, 
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
            }
        }

        public async Task<decimal> GetTotalPnlAsync(string accountId, string assetPairId, DateTime? closeTimeStart = null,
            DateTime? closeTimeEnd = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var whereClause = "WHERE 1=1 "
                             + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                             + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId = @assetPairId")
                             + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                             + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
                
                var query = $"SELECT SUM(Fpl) FROM {TableName} {whereClause}";
                
                _log.WriteInfoAsync(nameof(DealsSqlRepository), nameof(GetTotalPnlAsync),
                    new
                    {
                        accountId,
                        assetPairId,
                        closeTimeStart,
                        closeTimeEnd,
                        query
                    }.ToJson(), $"{nameof(GetTotalPnlAsync)} execution details");

                return await conn.QuerySingleOrDefaultAsync<decimal>(query,
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
            }
        }
    }
}

