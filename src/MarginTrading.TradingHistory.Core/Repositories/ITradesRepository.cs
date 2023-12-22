// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Domain;

namespace MarginTrading.TradingHistory.Core.Repositories
{
    public interface ITradesRepository
    {
        Task<ITrade> GetAsync(string tradeId);
        Task<IEnumerable<ITrade>> GetByAccountAsync([NotNull] string accountId, [CanBeNull] string assetPairId = null);
        Task<PaginatedResponse<ITrade>> GetByPagesAsync(string accountId, string assetPairId,
            int? skip = null, int? take = null, bool isAscending = true);

        Task SetCancelledByAsync(string cancelledTradeId, string cancelledBy);

        Task<IEnumerable<string>> GetMostTradedProductsAsync(DateTime date, int? max);
    }
}
