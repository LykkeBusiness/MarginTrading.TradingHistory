using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Client.Models;
using Refit;

namespace MarginTrading.TradingHistory.Client
{
    [PublicAPI]
    public interface IActivitiesApi
    {
        /// <summary>
        /// Get performed activities with optional filtering
        /// </summary>
        [Get("/api/activityHistory")]
        Task<List<ActivityContract>> ActivityHistory(
            [Query, CanBeNull] string accountId,
            [Query, CanBeNull] string instrument);
    }
}
