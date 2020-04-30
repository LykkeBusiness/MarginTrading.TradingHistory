// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core.Extensions;
using Microsoft.Data.SqlClient;

namespace MarginTrading.TradingHistory.SqlRepositories
{
    public static class SqlExtensions
    {
        public static void CreateTableIfDoesntExists(this IDbConnection connection, string createQuery,
            string tableName)
        {
            connection.Open();
            try
            {
                // Check if table exists
                connection.ExecuteScalar($"select top 1 * from {tableName}");
            }
            catch (SqlException)
            {
                // Create table
                var query = string.Format(createQuery, tableName);
                connection.Query(query);
            }
            finally
            {
                connection.Close();
            }
        }

        public static void InitializeSqlObject(this string connectionString, string scriptFileName, ILog log = null)
        {
            var creationScript = FileExtensions.ReadFromFile(scriptFileName);
            
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Execute(creationScript);
                }
                catch (Exception ex)
                {
                    log?.WriteErrorAsync(typeof(SqlExtensions).FullName, nameof(InitializeSqlObject), 
                        scriptFileName, ex).Wait();
                    throw;
                }
            }
        }
        
        public static void ExecuteCreateOrAlter(this IDbConnection connection, string query)
        {
            connection.Open();
            try
            {
                connection.ExecuteScalar(query);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
