// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IDealWithCommissionParams : IDeal
    {
        decimal? OvernightFees { get; }
        decimal? Commission { get; }
        decimal? OnBehalfFee { get; }
        decimal? Taxes { get; }
    }
}
