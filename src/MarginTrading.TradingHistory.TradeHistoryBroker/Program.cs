using MarginTrading.TradingHistory.BrokerBase;

namespace MarginTrading.TradingHistory.TradeHistoryBroker
{
    public class Program: WebAppProgramBase<Startup>
    {
        public static void Main(string[] args)
        {
            RunOnPort(5013);
        }
    }
}
