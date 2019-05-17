using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
    public class DealsSqlRepository : IDealsRepository
    {
        public const string TableName = "Deals";

        #region SQL
        
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [bigint] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[DealId] [nvarchar](64) NOT NULL,
[Created] [datetime] NOT NULL,
[AccountId] [nvarchar](64) NOT NULL,
[AssetPairId] [nvarchar](64) NOT NULL,
[OpenTradeId] [nvarchar] (64) NOT NULL,
[OpenOrderType] [nvarchar] (64) NULL,
[OpenOrderVolume] [float] NULL,
[OpenOrderExpectedPrice] [float] NULL,
[CloseTradeId] [nvarchar] (64) NULL,
[CloseOrderType] [nvarchar] (64) NULL,
[CloseOrderVolume] [float] NULL,
[CloseOrderExpectedPrice] [float] NULL,
[Direction] [nvarchar] (64) NOT NULL,
[Volume] [float] NULL,
[Originator] [nvarchar] (64) NOT NULL,
[OpenPrice] [float] NULL,
[OpenFxPrice] [float] NULL,
[ClosePrice] [float] NULL,
[CloseFxPrice] [float] NULL,
[Fpl] [float] NULL,
[PnlOfTheLastDay] [float] NULL,
[AdditionalInfo] [nvarchar](MAX) NULL ,
[OvernightFees] [float] NULL,
[Commission] [float] NULL,
[OnBehalfFee] [float] NULL,
[Taxes] [float] NULL,
INDEX IX_{0}_Base (DealId, AccountId, AssetPairId, Created)
);
GO

";

        private const string ProcedureScript = @"
CREATE OR ALTER PROCEDURE [dbo].[SP_UpdateDealCommissionInfo] (
  @eventSourceId nvarchar(64),
  @reasonType nvarchar(64)
)
AS
  BEGIN
    SET NOCOUNT ON;

    IF @reasonType = 'Swap'
      BEGIN
        UPDATE [dbo].[Deals]
        SET [OvernightFees] =
          (SELECT CONVERT(DECIMAL(24,13), Sum(swapHistory.SwapValue / ABS(swapHistory.Volume)) * ABS(deal.Volume))
            FROM dbo.[Deals] AS deal,
                 dbo.PositionsHistory AS position,
                 dbo.OvernightSwapHistory AS swapHistory
            WHERE position.Id = @eventSourceId
              AND deal.DealId = position.DealId
              AND position.Id = swapHistory.PositionId AND swapHistory.IsSuccess = 1
            GROUP BY deal.DealId, ABS(deal.Volume)
          )
        FROM dbo.PositionsHistory AS position
        WHERE [Deals].DealId = position.DealId AND position.Id = @eventSourceId
      END

    IF @reasonType = 'Commission'
      BEGIN
        WITH selectedAccounts AS
        (
          SELECT account.EventSourceId,
            account.ReasonType,
            account.ChangeAmount
          FROM dbo.[Deals] AS deal, dbo.AccountHistory AS account
          WHERE(account.EventSourceId IN (deal.OpenTradeId, deal.CloseTradeId) AND account.ReasonType = 'Commission')
        )
        UPDATE [dbo].[Deals]
        SET [Commission] = data.amount
        FROM (
            SELECT DISTINCT deal.DealId, CONVERT(DECIMAL(24,13), ((ISNULL(openingCommission.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                                              + ISNULL(closingCommission.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                                              * ABS(deal.Volume))) amount
            FROM dbo.[Deals] AS deal
            INNER JOIN selectedAccounts openingCommission
              ON deal.OpenTradeId = openingCommission.EventSourceId AND openingCommission.ReasonType = 'Commission'
            LEFT OUTER JOIN selectedAccounts closingCommission
              ON deal.CloseTradeId = closingCommission.EventSourceId AND closingCommission.ReasonType = 'Commission'
            WHERE deal.OpenTradeId = @eventSourceId OR deal.CloseTradeId = @eventSourceId
          ) data
        WHERE [dbo].[Deals].DealId = data.DealId
      END

    IF @reasonType = 'OnBehalf'
      BEGIN
        WITH selectedAccounts AS
        (
          SELECT DISTINCT account.EventSourceId,
            account.ReasonType,
            account.ChangeAmount
          FROM dbo.[Deals] AS deal, dbo.AccountHistory AS account
          WHERE(account.EventSourceId IN (deal.OpenTradeId, deal.CloseTradeId) AND account.ReasonType = 'OnBehalf')
        )
        UPDATE [dbo].[Deals]
        SET [OnBehalfFee] = data.amount
        FROM (
            SELECT DISTINCT deal.DealId, CONVERT(DECIMAL(24,13), ((ISNULL(openingOnBehalf.ChangeAmount, 0.0) / ABS(deal.OpenOrderVolume)
                                               + ISNULL(closingOnBehalf.ChangeAmount, 0.0) / ABS(deal.CloseOrderVolume))
                                              * ABS(deal.Volume))) amount
            FROM [dbo].[Deals] deal
            LEFT OUTER JOIN selectedAccounts openingOnBehalf
              ON deal.OpenTradeId = openingOnBehalf.EventSourceId AND openingOnBehalf.ReasonType = 'OnBehalf'
            LEFT OUTER JOIN selectedAccounts closingOnBehalf
              ON deal.CloseTradeId = closingOnBehalf.EventSourceId AND closingOnBehalf.ReasonType = 'OnBehalf'
            WHERE deal.OpenTradeId = @eventSourceId OR deal.CloseTradeId = @eventSourceId
          ) data
        WHERE [dbo].[Deals].DealId = data.DealId
      END

    IF @reasonType = 'Tax'
      BEGIN
        UPDATE [dbo].[Deals]
        SET [Taxes] =
          (
            SELECT CONVERT(DECIMAL(24,13), ISNULL(account.ChangeAmount, 0.0))
            FROM [dbo].[Deals] deal, [dbo].[AccountHistory] account
            WHERE account.EventSourceId = deal.DealId AND account.ReasonType = 'Tax'
            AND deal.DealId = @eventSourceId
          )
        WHERE [Deals].DealId = @eventSourceId -- it could also be CompensationId, so it is automatically skipped
      END
  END
";

        private const string AccountTriggerScript = @" 
CREATE OR ALTER TRIGGER [dbo].[T_InsertAccountTransaction] ON [dbo].[AccountHistory]
  AFTER INSERT
AS
  BEGIN
    SET XACT_ABORT OFF;
    SET NOCOUNT ON;

    DECLARE @eventSourceId nvarchar(64), @reasonType nvarchar(64)

    SELECT @eventSourceId = [EventSourceId], @reasonType = [ReasonType]
    FROM INSERTED

    IF @eventSourceId IS NULL AND @reasonType IN ('Swap', 'Commission', 'OnBehalf', 'Tax')
      RAISERROR('EventSourceId was null, reason type [%s]', 1, 1, @reasonType);

    BEGIN TRY
      EXEC [dbo].[SP_UpdateDealCommissionInfo] @eventSourceId, @reasonType
    END TRY
    BEGIN CATCH
      DECLARE @ErrorMessage NVARCHAR(4000);

      SELECT @ErrorMessage = 'Failed to update deal ' + @reasonType + ERROR_MESSAGE();

      RAISERROR(@ErrorMessage, 1, 1);
    END CATCH

    SET XACT_ABORT ON;
  END;
";


        private const string DealTriggerScript = @" 
CREATE OR ALTER TRIGGER [dbo].[T_InsertDeal] ON [dbo].[Deals]
  AFTER INSERT
AS
  BEGIN
    SET XACT_ABORT OFF;
    SET NOCOUNT ON;

    DECLARE @positionId nvarchar(64), @dealId nvarchar(64), @closeTradeId nvarchar(64), @ErrorMessage NVARCHAR(4000)

    SELECT @dealId = [DealId], @closeTradeId = [CloseTradeId]
    FROM INSERTED

    BEGIN TRY
      SET @positionId = (SELECT TOP 1 ph.Id
      FROM [dbo].[PositionsHistory] ph, [dbo].[Deals] d
      WHERE d.DealId = ph.DealId AND d.DealId = @dealId);

      IF @positionId IS NULL
        RAISERROR('Position was not found by deal id [%s]', 1, 1, @dealId);

      EXEC [dbo].[SP_UpdateDealCommissionInfo] @positionId, 'Swap'
    END TRY
    BEGIN CATCH
      SELECT @ErrorMessage = 'Failed to update [%s] deal Swap. ' + ERROR_MESSAGE();
    END CATCH

    BEGIN TRY
      EXEC [dbo].[SP_UpdateDealCommissionInfo] @closeTradeId, 'Commission'
    END TRY
    BEGIN CATCH
      SELECT @ErrorMessage = 'Failed to update [%s] deal Commission. ' + ERROR_MESSAGE();
    END CATCH

    BEGIN TRY
      EXEC [dbo].[SP_UpdateDealCommissionInfo] @closeTradeId, 'OnBehalf'
    END TRY
    BEGIN CATCH
      SELECT @ErrorMessage = 'Failed to update [%s] deal OnBehalf. ' + ERROR_MESSAGE();
    END CATCH

    BEGIN TRY
      EXEC [dbo].[SP_UpdateDealCommissionInfo] @dealId, 'Tax'
    END TRY
    BEGIN CATCH
      SELECT @ErrorMessage = 'Failed to update deal Tax. ' + ERROR_MESSAGE();
    END CATCH

    IF @ErrorMessage IS NOT NULL
      BEGIN
        RAISERROR(@ErrorMessage, 1, 1);
      END

    SET XACT_ABORT ON;
  END;
"; 
        
        #endregion SQL

        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly Func<PropertyInfo, bool> FilterTriggerInjectedFieldsPredicate = pi => 
            !new [] {nameof(IDeal.OvernightFees), nameof(IDeal.Commission), nameof(IDeal.OnBehalfFee), 
                nameof(IDeal.Taxes)}.Contains(pi.Name);
        
        public static readonly string GetColumns = string.Join(",", 
            typeof(IDeal).GetProperties().Where(FilterTriggerInjectedFieldsPredicate).Select(x => x.Name));

        public static readonly string GetFields = string.Join(",", 
            typeof(IDeal).GetProperties().Where(FilterTriggerInjectedFieldsPredicate).Select(x => "@" + x.Name));

        public DealsSqlRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try 
                { 
                    conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
                    conn.ExecuteCreateOrAlter(ProcedureScript);
                    conn.ExecuteCreateOrAlter(AccountTriggerScript);
                    conn.ExecuteCreateOrAlter(DealTriggerScript);
                }
                catch (Exception ex)
                {
                  _log?.WriteErrorAsync(nameof(DealsSqlRepository), "Create table and triggers", null, ex);
                    throw;
                }
            }
        }
        
        public async Task<IDeal> GetAsync(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = $"SELECT * FROM {TableName} WHERE DealId = @id";
                var objects = await conn.QueryAsync<DealEntity>(query, new {id});
                
                return objects.FirstOrDefault();
            }
        }

        public async Task<PaginatedResponse<IDeal>> GetByPagesAsync(string accountId, string assetPairId,
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
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
                var deals = (await gridReader.ReadAsync<DealEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();
            
                return new PaginatedResponse<IDeal>(
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
            var paginationClause = $" ORDER BY [AssetPairId] {order} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";

            using (var conn = new SqlConnection(_connectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $@"SELECT {nameof(IAggregatedDeal.AccountId)},
                        {nameof(IAggregatedDeal.AssetPairId)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.Volume)})) AS {nameof(IAggregatedDeal.Volume)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.Fpl)})) AS {nameof(IAggregatedDeal.Fpl)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.Fpl)} / {nameof(IDeal.CloseFxPrice)})) AS {nameof(IAggregatedDeal.FplTc)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.PnlOfTheLastDay)})) AS {nameof(IAggregatedDeal.PnlOfTheLastDay)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.OvernightFees)})) AS {nameof(IAggregatedDeal.OvernightFees)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.Commission)})) AS {nameof(IAggregatedDeal.Commission)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.OnBehalfFee)})) AS {nameof(IAggregatedDeal.OnBehalfFee)},
                        CONVERT(DECIMAL(24, 13), SUM({nameof(IDeal.Taxes)})) AS {nameof(IAggregatedDeal.Taxes)}
                      FROM {TableName}
                      {whereClause}
                      GROUP BY {nameof(IAggregatedDeal.AccountId)}, {nameof(IAggregatedDeal.AssetPairId)}
                      {paginationClause}; SELECT COUNT(DISTINCT {nameof(IAggregatedDeal.AssetPairId)}) FROM {TableName} {whereClause}",
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

        public async Task<IEnumerable<IDeal>> GetAsync(string accountId, string assetPairId,
            DateTime? closeTimeStart = null, DateTime? closeTimeEnd = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var clause = "WHERE 1=1 "
                    + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND AccountId = @accountId")
                    + (string.IsNullOrWhiteSpace(assetPairId) ? "" : " AND AssetPairId = @assetPairId")
                    + (closeTimeStart == null ? "" : " AND Created >= @closeTimeStart")
                    + (closeTimeEnd == null ? "" : " AND Created < @closeTimeEnd");
                
                var query = $"SELECT * FROM {TableName} {clause}";
                return await conn.QueryAsync<DealEntity>(query, 
                    new {accountId, assetPairId, closeTimeStart, closeTimeEnd});
            }
        }

        public async Task AddAsync(IDeal obj)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    var entity = DealEntity.Create(obj);
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              $"Entity <{nameof(IDeal)}>: \n" +
                              obj.ToJson();
                    
                    _log?.WriteWarning(nameof(DealsSqlRepository), nameof(AddAsync), msg);
                    
                    throw new Exception(msg);
                }
            }
        }
    }
}

