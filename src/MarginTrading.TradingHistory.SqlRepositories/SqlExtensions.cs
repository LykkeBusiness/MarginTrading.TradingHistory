// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using Common.Log;
using Dapper;
using MarginTrading.TradingHistory.Core.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// Runs SQL objects initializations scripts.
        /// Potentially it can be long running operation therefore optional command timeout can be provided.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="scriptFileName">Script file name</param>
        /// <param name="commandTimeout">Command timeout in seconds, infinite by default</param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException">When <paramref name="scriptFileName"/> is null or empty</exception>
        public static void InitializeSqlObject(this string connectionString,
            string scriptFileName,
            int commandTimeout = 0,
            ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(scriptFileName))
                throw new ArgumentNullException(nameof(scriptFileName));

            var creationScript = FileExtensions.ReadFromFile(scriptFileName);

            using var conn = new SqlConnection(connectionString);
            try
            {
                conn.Execute(creationScript, commandTimeout: commandTimeout);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, scriptFileName);
                throw;
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
