using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.TradingHistory.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderRejectReasonContract
    {
        None,
        NoLiquidity,
        NotEnoughBalance,
        LeadToStopOut,
        AccountInvalidState,
        InvalidExpectedOpenPrice,
        InvalidVolume,
        InvalidTakeProfit,
        InvalidStoploss,
        InvalidInstrument,
        InvalidAccount,
        InvalidParent,
        TradingConditionError,
        InvalidValidity,
        TechnicalError,
        ParentPositionDoesNotExist,
        ParentPositionIsNotActive,
        ShortPositionsDisabled,
        MaxPositionLimit,
        MinOrderSizeLimit,
        MaxOrderSizeLimit,
    }
}
