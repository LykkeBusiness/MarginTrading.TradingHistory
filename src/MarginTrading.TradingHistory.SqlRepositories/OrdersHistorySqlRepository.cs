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
        private const string TableName = "OrdersChangeHistory";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [int] NOT NULL IDENTITY (1,1) PRIMARY KEY,
[Id] [nvarchar](64) NOT NULL,
[Code] [bigint] NULL,
[ClientId] [nvarchar] (64) NOT NULL,
[TradingConditionId] [nvarchar] (64) NOT NULL,
[AccountAssetId] [nvarchar] (64) NULL,
[Instrument] [nvarchar] (64) NOT NULL,
[Type] [nvarchar] (64) NOT NULL,
[CreateDate] [datetime] NOT NULL,
[OpenDate] [datetime] NULL,
[CloseDate] [datetime] NULL,
[ExpectedOpenPrice] [float] NULL,
[OpenPrice] [float] NULL,
[ClosePrice] [float] NULL,
[QuoteRate] [float] NULL,
[Volume] [float] NULL,
[TakeProfit] [float] NULL,
[StopLoss] [float] NULL,
[CommissionLot] [float] NULL,
[OpenCommission] [float] NULL,
[CloseCommission] [float] NULL,
[SwapCommission] [float] NULL,
[EquivalentAsset] [nvarchar] (64) NULL,
[OpenPriceEquivalent] [float] NULL,
[ClosePriceEquivalent] [float] NULL,
[StartClosingDate] [datetime] NULL,
[Status] [nvarchar] (64) NULL,
[CloseReason] [nvarchar] (64) NULL,
[FillType] [nvarchar] (64) NULL,
[RejectReason] [nvarchar] (64) NULL,
[RejectReasonText] [nvarchar] (255) NULL,
[Comment] [nvarchar] (255) NULL,
[MatchedVolume] [float] NULL,
[MatchedCloseVolume] [float] NULL,
[Fpl] [float] NULL,
[PnL] [float] NULL,
[InterestRateSwap] [float] NULL,
[MarginInit] [float] NULL,
[MarginMaintenance] [float] NULL,
[OrderUpdateType] [nvarchar] (64) NULL,
[OpenExternalOrderId] [nvarchar] (64) NULL,
[OpenExternalProviderId] [nvarchar] (64) NULL,
[CloseExternalOrderId] [nvarchar] (64) NULL,
[CloseExternalProviderId] [nvarchar] (64) NULL,
[MatchingEngineMode] [nvarchar] (64) NULL,
[LegalEntity] [nvarchar] (64) NULL),
[ParentPositionId] [nvarchar](64) NULL,
[ParentOrderId] [nvarchar](64) NULL;";

        private readonly string _reportsSqlConnString;
        private readonly ILog _log;

        private static readonly string GetColumns =
            string.Join(",", typeof(IOrderHistory).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(IOrderHistory).GetProperties().Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            typeof(IOrderHistory).GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        public OrdersHistorySqlRepository(string reportsSqlConnString, ILog log)
        {
            _reportsSqlConnString = reportsSqlConnString;
            _log = log;
            
            using (var conn = new SqlConnection(reportsSqlConnString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync("OrdersChangeHistory", "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(OrderHistory order)
        {
            using (var conn = new SqlConnection(_reportsSqlConnString))
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

        public Task<IEnumerable<OrderHistory>> GetHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<OrderHistory>> GetHistoryAsync(string[] accountIds, DateTime? @from, DateTime? to)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderHistory>> GetHistoryAsync(Func<OrderHistory, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
