﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;

using Lykke.SettingsReader.ConfigurationProvider;

using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.TradingHistory
{
    [UsedImplicitly]
    internal sealed class Program
    {
        internal static IHost AppHost { get; private set; }
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
                    var configurationBuilder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddUserSecrets<Startup>()
                        .AddEnvironmentVariables();

                    if (Environment.GetEnvironmentVariable("SettingsUrl")?.StartsWith("http") ?? false)
                    {
                        configurationBuilder.AddHttpSourceConfiguration();
                    }

                    var configuration = configurationBuilder.Build();

                    AppHost = Host.CreateDefaultBuilder(args)
                        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.ConfigureKestrel(serverOptions =>
                                {
                                    // Set properties and call methods on options
                                })
                                .UseConfiguration(configuration)
                                .UseStartup<Startup>();
                        })
                        .Build();

                    await AppHost.RunAsync();
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