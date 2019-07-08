// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.TradingHistory.Client.Models
{
    public enum FxToAssetPairDirectionContract
    {
        /// <summary>
        /// AssetPair is {BaseId, QuoteId} and FxAssetPair is {QuoteId, AccountAssetId}
        /// </summary>
        Straight,
        
        /// <summary>
        /// AssetPair is {BaseId, QuoteId} and FxAssetPair is {AccountAssetId, QuoteId}
        /// </summary>
        Reverse,
    }
}
