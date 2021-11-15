// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Core.Domain
{
    public interface IOrderHistoryForOrderBlotterWithAdditionalData: IOrderHistoryForOrderBlotter
    {
        string AssetName { get; }
        decimal? TakeProfitPrice { get; }
        decimal? StopLossPrice { get; }
        decimal? Commission { get; }
        decimal? OnBehalfFee { get; }
        decimal? Spread { get; }
    }
}
