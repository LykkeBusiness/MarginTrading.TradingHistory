// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Lykke.Snow.Common;
using MarginTrading.TradingHistory.Core;
using MarginTrading.TradingHistory.Core.Domain;
using MarginTrading.TradingHistory.Core.Repositories;
using MarginTrading.TradingHistory.SqlRepositories.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class DealsSqlRepository : SqlRepositoryBase, IDealsRepository
    {
        private const string ViewName = "[dbo].[V_DealsWithCommissionParams]";
        private const string TableName = "[dbo].[Deals]";
        private const string GetDealDetailsProcName = "[dbo].[getDealDetails]";
        private readonly string _connectionString;

        public static readonly List<string> DealInsertColumns = typeof(IDeal).GetProperties().Select(x => x.Name).ToList();

        public DealsSqlRepository(string connectionString, ILogger<DealsSqlRepository> logger)
            : base(connectionString, logger)
        {
            _connectionString = connectionString;

            connectionString.InitializeSqlObject("dbo.Deals.sql", logger);
            connectionString.InitializeSqlObject("dbo.DealCommissionParams.sql", logger);
            connectionString.InitializeSqlObject("dbo.getDealDetails.sql", logger);
            connectionString.InitializeSqlObject("dbo.V_DealsWithCommissionParams.sql", logger);
        }
        
        public async Task<IDealWithCommissionParams> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {ViewName} WITH (NOLOCK) WHERE DealId = @id";
                var objects = await conn.QueryAsync<DealWithCommissionParamsEntity>(query, new {id});
                return objects.FirstOrDefault();
            }
        }
        
        public async Task<IDealDetails> GetDetailsAsync(string id)
        {
            return await GetAsync(
                GetDealDetailsProcName,
                new[] { new SqlParameter(nameof(IDealDetails.DealId), id) },
                Map);
        }

        private DealDetailsEntity Map(SqlDataReader reader)
        {
            return new DealDetailsEntity
            {
                AccountId = reader[nameof(DealDetailsEntity.AccountId)] as string,
                AssetPairId = reader[nameof(DealDetailsEntity.AssetPairId)] as string,
                PositionId = reader[nameof(DealDetailsEntity.PositionId)] as string,
                DealId = reader[nameof(DealDetailsEntity.DealId)] as string,
                DealSize = (reader[nameof(DealDetailsEntity.DealSize)] as int?).GetValueOrDefault(),
                DealTimestamp = (reader[nameof(DealDetailsEntity.DealTimestamp)] as DateTime?).GetValueOrDefault(),
                OpenTradeId = reader[nameof(DealDetailsEntity.OpenTradeId)] as string,
                OpenTimestamp = (reader[nameof(DealDetailsEntity.OpenTimestamp)] as DateTime?).GetValueOrDefault(),
                OpenDirection = reader[nameof(DealDetailsEntity.OpenDirection)] as string,
                OpenSize = (reader[nameof(DealDetailsEntity.OpenSize)] as int?).GetValueOrDefault(),
                OpenPrice = (reader[nameof(DealDetailsEntity.OpenPrice)] as decimal?).GetValueOrDefault(),
                OpenContractVolume = (reader[nameof(DealDetailsEntity.OpenContractVolume)] as decimal?).GetValueOrDefault(),
                TotalOpenDirection = reader[nameof(DealDetailsEntity.TotalOpenDirection)] as string,
                TotalOpenSize = (reader[nameof(DealDetailsEntity.TotalOpenSize)] as int?).GetValueOrDefault(),
                TotalOpenPrice = (reader[nameof(DealDetailsEntity.TotalOpenPrice)] as decimal?).GetValueOrDefault(),
                TotalOpenContractVolume = (reader[nameof(DealDetailsEntity.TotalOpenContractVolume)] as decimal?).GetValueOrDefault(),
                CloseTradeId = reader[nameof(DealDetailsEntity.CloseTradeId)] as string,
                CloseTimestamp = (reader[nameof(DealDetailsEntity.CloseTimestamp)] as DateTime?).GetValueOrDefault(),
                CloseDirection = reader[nameof(DealDetailsEntity.CloseDirection)] as string,
                CloseSize = (reader[nameof(DealDetailsEntity.CloseSize)] as int?).GetValueOrDefault(),
                ClosePrice = (reader[nameof(DealDetailsEntity.ClosePrice)] as decimal?).GetValueOrDefault(),
                CloseContractVolume = (reader[nameof(DealDetailsEntity.CloseContractVolume)] as decimal?).GetValueOrDefault(),
                GrossPnLTc = (reader[nameof(DealDetailsEntity.GrossPnLTc)] as decimal?).GetValueOrDefault(),
                GrossPnLFxPrice = (reader[nameof(DealDetailsEntity.GrossPnLFxPrice)] as decimal?).GetValueOrDefault(),
                GrossPnLSc = (reader[nameof(DealDetailsEntity.GrossPnLSc)] as decimal?).GetValueOrDefault(),
                OverallOnBehalfFees = (reader[nameof(DealDetailsEntity.OverallOnBehalfFees)] as decimal?).GetValueOrDefault(),
                OverallFinancingCost = (reader[nameof(DealDetailsEntity.OverallFinancingCost)] as decimal?).GetValueOrDefault(),
                OverallCommissions = (reader[nameof(DealDetailsEntity.OverallCommissions)] as decimal?).GetValueOrDefault(),
                RealizedPnLDaySc = (reader[nameof(DealDetailsEntity.RealizedPnLDaySc)] as decimal?).GetValueOrDefault(),
                RealisedPnLBtxSc = (reader[nameof(DealDetailsEntity.RealisedPnLBtxSc)] as decimal?).GetValueOrDefault(),
                NettingOfPreviouslySettledPnLs = (reader[nameof(DealDetailsEntity.NettingOfPreviouslySettledPnLs)] as decimal?).GetValueOrDefault(),
                TaxInfo = reader[nameof(DealDetailsEntity.TaxInfo)] as string,
            };
        }

        public async Task<PaginatedResponse<IDealWithCommissionParams>> GetByPagesAsync(string accountId,
            string assetPairId, List<PositionDirection> directions,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true)
        {
            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            var whereClause = "WHERE 1=1 "
                              + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId=@accountId")
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (directions?.Count == 0 ? "" : " AND Direction IN @directions")
                              + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                              + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [Created] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {take} ROWS ONLY";
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {ViewName} WITH (NOLOCK) {whereClause} {paginationClause}; SELECT COUNT(*) FROM {ViewName} WITH (NOLOCK) {whereClause}",
                    new {accountId, assetPairId, directions = directions.Select(x => x.ToString()), closeTimeStart, closeTimeEnd});
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

        public async Task<PaginatedResponse<IAggregatedDeal>> GetAggregated(string accountId,
            string assetPairId, List<PositionDirection> directions,
            DateTime? closeTimeStart, DateTime? closeTimeEnd,
            int? skip = null, int? take = null, bool isAscending = true)
        {

            (skip, take) = PaginationUtils.ValidateSkipAndTake(skip, take);

            var whereClause = "WHERE AccountId=@accountId"
                              + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId=@assetPairId")
                              + (directions?.Count == 0 ? "" : " AND Direction IN @directions")
                              + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                              + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
            var order = isAscending ? string.Empty : Constants.DescendingOrder;
            var paginationClause = $" ORDER BY [{nameof(IAggregatedDeal.LastDealDate)}] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {take} ROWS ONLY";

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
                      FROM {ViewName} WITH (NOLOCK)
                      {whereClause}
                      GROUP BY {nameof(IAggregatedDeal.AccountId)}, {nameof(IAggregatedDeal.AssetPairId)}
                      {paginationClause}; SELECT COUNT(DISTINCT {nameof(IAggregatedDeal.AssetPairId)}) FROM {ViewName} WITH (NOLOCK) {whereClause}",
                    new { accountId, assetPairId, directions = directions.Select(x => x.ToString()), closeTimeStart, closeTimeEnd });
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
                
                var query = $"SELECT * FROM {ViewName} WITH (NOLOCK) {clause}";
                return await conn.QueryAsync<DealWithCommissionParamsEntity>(query, 
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
            }
        }

        public async Task<decimal> GetTotalPnlAsync(string accountId,
            string assetPairId, List<PositionDirection> directions,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var whereClause = "WHERE 1=1 "
                             + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                             + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId = @assetPairId")
                             + (directions?.Count == 0 ? "" : " AND Direction IN @directions")
                             + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                             + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
                
                var query = $"SELECT ISNULL(SUM(Fpl), 0) FROM {TableName} WITH (NOLOCK) {whereClause}";

                return await conn.QuerySingleOrDefaultAsync<decimal>(query,
                    new {accountId, assetPairId, directions = directions.Select(x => x.ToString()), closeTimeStart, closeTimeEnd});
            }
        }

        public async Task<decimal> GetTotalProfitAsync(string accountId, DateTime[] days)
        {
            if (string.IsNullOrEmpty(accountId))
                throw new ArgumentNullException(nameof(accountId));

            if (days == null || days.Length == 0)
                throw new ArgumentNullException(nameof(days));
            
            using (var conn = new SqlConnection(_connectionString))
            {
                var query =
                    $"SELECT ISNULL(SUM(p.[Value]),0) " +
                    $"FROM (SELECT SUM(Fpl) as [Value] FROM {TableName} " +
                    $"WHERE AccountId = @accountId AND CAST([Created] as DATE) IN @days AND Fpl > 0" +
                    $"GROUP BY CAST([Created] as DATE)) as p";
                    
                return await conn.QuerySingleOrDefaultAsync<decimal>(query, new {accountId, days});
            }
        }
    }
}

