// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Data;
using System.Threading.Tasks;
using Common.Log;
using Dapper;
using Lykke.Common.MsSql;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public static class DapperExtensions
    {
        /// <summary>
        /// Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="ignoreDuplicates">The unique key constraint violation will be logged as warning</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<int> ExecuteAsync(this IDbConnection cnn,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            bool ignoreDuplicates = false,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (ignoreDuplicates)
            {
                try
                {
                    return await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
                }
                catch (SqlException e) when (e.Number.IsDuplicateKeyViolation())
                {
                    var id = param?
                        .GetType()
                        .GetProperty("Id")?
                        .GetValue(param)?
                        .ToString();

                    LogLocator.Log.WriteWarning(nameof(ExecuteAsync), "",
                        "An attempt to add duplicate entry" + (string.IsNullOrEmpty(id) ? "" : " for " + id));

                    return 0;
                }
            }
            
            return await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }
    }
}
