namespace MarginTrading.TradingHistory.Client.Models
{
    public class LimitOrderContract : BaseOrderContract
    {
        public string MarketMakerId { get; set; }
        public decimal Price { get; set; }
    }
}
