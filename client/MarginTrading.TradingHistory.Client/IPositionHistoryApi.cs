using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface IPositionHistoryApi
    {
        [Get("/api/positionHistory/{accountId}")]
        Task<List<PositionContract>> List(
            [NotNull] string accountId);
    }
}
