// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.TradingHistory
{
    public static class ApiValidationHelper
    {
        public static void ValidatePagingParams(int? skip, int? take)
        {
            if ((skip.HasValue && !take.HasValue) || (!skip.HasValue && take.HasValue))
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Both skip and take must be set or unset");
            }

            if (take.HasValue && (take <= 0 || skip < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be >= 0, take must be > 0");
            }
        }

        public static void ValidateAggregatedParams(string accountId, int? skip, int? take)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentNullException(nameof(accountId), $"{nameof(accountId)} must be provided");
            }

            ValidatePagingParams(skip, take);
        }
    }
}
