// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Data;
using Dapper;

namespace MarginTrading.TradingHistory.DapperExtensions
{
    public static class DapperHandlers
    {
        public static void Register()
        {
            SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
        }
    }
}
