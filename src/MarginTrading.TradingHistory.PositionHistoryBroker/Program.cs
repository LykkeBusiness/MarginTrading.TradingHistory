using MarginTrading.TradingHistory.BrokerBase;

namespace MarginTrading.TradingHistory.PositionHistoryBroker
{
    public class Program: WebAppProgramBase<Startup>
    {
        public static void Main(string[] args)
        {
            RunOnPort(5014);
        }
    }
}
