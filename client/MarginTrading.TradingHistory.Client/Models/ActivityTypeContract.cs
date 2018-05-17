namespace MarginTrading.TradingHistory.Client.Models
{
    public enum ActivityTypeContract
    {
        Unknown = 0,
        OrderAcceptance = 1,    // OrderStatus.Inactive
        OrderActivation = 2,    // OrderStatus.Active
        OrderCancellation = 3,  // OrderStatus.Cancelled
        OrderExecution = 4,     // OrderStatus.Executed
        OrderExpiry = 5,        // OrderStatus.Expired
        OrderModification = 6,
        OrderRejection = 7,
        PositionOpening = 8,
        PositionClosing = 9
    }
}
