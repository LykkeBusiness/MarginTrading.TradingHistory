// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.Core.Extensions
{
    public static class TransactionWrapperExtensions
    {
        public static async Task RunInTransactionAsync(
            Func<SqlConnection, SqlTransaction, Task> action,
            string connectionString,
            Func<Exception, Task> logRollbackExceptionHandler = null,
            Func<Exception, Task> logCommitExceptionHandler = null,
            IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (action == null)
                return;
            
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                
                var transaction = conn.BeginTransaction(isolationLevel);

                try
                {
                    await action(conn, transaction);
                    
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        if (logRollbackExceptionHandler != null)
                            await logRollbackExceptionHandler(rollbackEx);
                    }

                    if (logCommitExceptionHandler != null)
                        await logCommitExceptionHandler(ex);
                    
                    throw;
                }
            }
        }
    }
}
