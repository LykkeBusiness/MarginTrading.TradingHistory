using System.Threading.Tasks;
using Lykke.SlackNotifications;

namespace MarginTrading.TradingHistory.Core.Services
{
    public interface IMtSlackNotificationsSender : ISlackNotificationsSender
    {
        Task SendRawAsync(string type, string sender, string message);
    }
}
