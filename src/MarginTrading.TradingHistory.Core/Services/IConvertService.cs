// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Core.Services
{
    public interface IConvertService
    {
        TResult Convert<TSource, TResult>(TSource source);
        void AssertConfigurationIsValid();
    }
}
