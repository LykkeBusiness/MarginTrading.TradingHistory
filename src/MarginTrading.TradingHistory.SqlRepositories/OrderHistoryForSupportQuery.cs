using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using MarginTrading.TradingHistory.Core.Domain;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public class OrderHistoryForSupportQuery
    {
        public class ResultItem
        {
            public string OrderId { get; set; }

            public string ClientId { get; set; }

            public DateTime ExecutedTimestamp { get; set; }

            public DateTime CreatedTimestamp { get; set; }

            public string Status { get; set; }

            public decimal Volume { get; set; }

            public decimal ExecutionPrice { get; set; }

            public string AssetPairId { get; set; }
        }

        public class Criterion
        {
            public string Id { get; set; }

            public DateTime? ExecutedTimestampFrom { get; set; }

            public DateTime? ExecutedTimestampTo { get; set; }

            public DateTime? CreatedTimestampFrom { get; set; }

            public DateTime? CreatedTimestampTo { get; set; }

            public IEnumerable<string> AssetPairIds { get; set; }

            public string ClientId { get; set; }

            public decimal? ExecutionPrice { get; set; }

            public int Skip { get; set; }

            public int Take { get; set; }

            public bool IsAscending { get; set; }
        }

        private readonly string _connectionString;
        private readonly TimeSpan _executionTimeout;

        public OrderHistoryForSupportQuery(string connectionString, TimeSpan executionTimeout)
        {
            _connectionString = connectionString;
            _executionTimeout = executionTimeout;
        }

        private readonly string _template = $@"
select
  oh.Id as '{nameof(ResultItem.OrderId)}',
  a.ClientId as '{nameof(ResultItem.ClientId)}',
  oh.ExecutedTimestamp as '{nameof(ResultItem.ExecutedTimestamp)}',
  oh.CreatedTimestamp as '{nameof(ResultItem.CreatedTimestamp)}',
  'created'  as '{nameof(ResultItem.Status)}',
  oh.Volume  as '{nameof(ResultItem.Volume)}',
  oh.ExecutionPrice  as '{nameof(ResultItem.ExecutionPrice)}',
  oh.AssetPairId  as '{nameof(ResultItem.AssetPairId)}'
from OrdersHistory oh
join MarginTradingAccounts a on oh.AccountId = a.Id 
/**where**/
/**orderby**/
offset @{nameof(Criterion.Skip)} ROWS FETCH NEXT @{nameof(Criterion.Take)} ROWS ONLY;
select count(*) from OrdersHistory oh join MarginTradingAccounts a on oh.AccountId = a.Id  /**where**/
";

        public async Task<PaginatedResponse<ResultItem>> Ask(Criterion criterion)
        {
            var builder = new SqlBuilder();
            var selector = builder.AddTemplate(_template);
            var parameters = GetDynamicParameters(criterion);
            FillParams(parameters, builder, criterion);

            using var connection = new SqlConnection(_connectionString);
            using var reader = await connection.QueryMultipleAsync(selector.RawSql, parameters, commandTimeout: (int) _executionTimeout.TotalSeconds);

            var items = await reader.ReadAsync<ResultItem>();
            var count = await reader.ReadSingleAsync<int>();

            var result = new PaginatedResponse<ResultItem>(items.ToList(), criterion.Skip, criterion.Take, count);

            return result;
        }

        private static void FillParams(DynamicParameters parameters, SqlBuilder builder, Criterion criterion)
        {
            if (criterion.ExecutedTimestampFrom != null)
            {
                builder.Where($"oh.ExecutedTimestamp >= @{nameof(criterion.ExecutedTimestampFrom)}");
            }
            if (criterion.ExecutedTimestampTo != null)
            {
                builder.Where($"oh.ExecutedTimestamp < @{nameof(criterion.ExecutedTimestampTo)}");
            }
            if (criterion.CreatedTimestampFrom != null)
            {
                builder.Where($"oh.CreatedTimestamp >= @{nameof(criterion.CreatedTimestampFrom)}");
            }
            if (criterion.CreatedTimestampTo != null)
            {
                builder.Where($"oh.CreatedTimestamp < @{nameof(criterion.CreatedTimestampTo)}");
            }
            if (criterion.AssetPairIds != null)
            {
                builder.Where($"oh.AssetPairId in @{nameof(criterion.AssetPairIds)}");
            }
            if (!string.IsNullOrEmpty(criterion.ClientId))
            {
                builder.Where($"a.ClientId = @{nameof(criterion.ClientId)}");
            }
            if (criterion.ExecutionPrice != null)
            {
                builder.Where($"oh.ExecutionPrice = @{nameof(criterion.ExecutionPrice)}");
            }
            if (!string.IsNullOrEmpty(criterion.Id))
            {
                builder.Where($"oh.Id = @{nameof(criterion.Id)}");
            }

            builder.OrderBy(criterion.IsAscending ? "oh.ExecutedTimestamp asc" : "oh.ExecutedTimestamp desc");
        }

        private static DynamicParameters GetDynamicParameters(object criterion)
        {
            var parameters = new DynamicParameters();
            foreach (var parameter in criterion.GetType()
                                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Select(p => (p.Name, Value: p.GetValue(criterion))))
            {
                parameters.Add(parameter.Name, parameter.Value);
            }

            return parameters;
        }
    }
}
