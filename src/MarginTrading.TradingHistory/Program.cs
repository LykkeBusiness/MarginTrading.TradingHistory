using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.TradingHistory
{
    [UsedImplicitly]
    internal sealed class Program
    {
        public static string EnvInfo => Environment.GetEnvironmentVariable("ENV_INFO");
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");
            
            var restartAttemptsLeft = int.TryParse(Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_NUMBER"),
                out var restartAttemptsFromEnv) 
                ? restartAttemptsFromEnv
                : int.MaxValue;
            var restartAttemptsInterval = int.TryParse(Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_INTERVAL_MS"),
                out var restartAttemptsIntervalFromEnv) 
                ? restartAttemptsIntervalFromEnv
                : 10000;

            while (restartAttemptsLeft > 0)
            {
                try
                {
                    var host = new WebHostBuilder()
                        .UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Any, 5040);
                            options.Listen(IPAddress.Any, 5041, SecurityHelpers.ConfigureHttpsEndpoint);
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseApplicationInsights()
                        .Build();

                    await host.RunAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}{Environment.NewLine}{e.StackTrace}{Environment.NewLine}Restarting...");
                    LogLocator.Log?.WriteFatalErrorAsync(
                        "MT TradingHistory", "Restart host", $"Attempts left: {restartAttemptsLeft}", e);
                    restartAttemptsLeft--;
                    Thread.Sleep(restartAttemptsInterval);
                }
            }

            Console.WriteLine("Terminated");
        }
    }
}
