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
    }
}
