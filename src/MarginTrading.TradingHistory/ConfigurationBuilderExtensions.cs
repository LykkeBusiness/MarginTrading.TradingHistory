using System;
using System.Collections.Generic;
using System.IO;
using Common.Log;
using MarginTrading.TradingHistory.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MarginTrading.TradingHistory
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddDevJson(this IConfigurationBuilder builder, IHostingEnvironment env)
        {
            return builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"SettingsUrl", Path.Combine(env.ContentRootPath, "appsettings.dev.json")}
            });
        }
        
        private static readonly List<(string, string, string)> EnvironmentSecretConfig = new List<(string, string, string)>
        {
            /* secrets.json Key             // Environment Variable     // default value (optional) */
            ("X509-Certificate-Path",       "X509_CERTIFICATE_PATH",       null),
            ("X509-Certificate-Password",   "X509_CERTIFICATE_PASSWORD",       null),
            ("Force-Https-Redirection",      "FORCE_HTTPS_REDIRECTION",       "false"),
        };

        public static IConfigurationBuilder AddEnvironmentSecrets(this IConfigurationBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables()
                .Build();

            var log = new LogToConsole();//todo these messages won't be logged to log output

            var secrets = new Dictionary<string, string>();

            foreach (var (key, environmentVariable, defaultValue) in EnvironmentSecretConfig)
            {
                secrets[key] = configuration[key];
                if (secrets[key] != null)
                {
                    log.WriteInfo(nameof(ConfigurationBuilderExtensions), nameof(AddEnvironmentSecrets), 
                        $"Value of {key} satisfied by secrets.json");
                    continue;
                }

                secrets[key] = configuration[environmentVariable];
                if (secrets[key] != null)
                {
                    log.WriteInfo(nameof(ConfigurationBuilderExtensions), nameof(AddEnvironmentSecrets), 
                        $"Value of {key} satisfied by environment variable");
                    continue;
                }

                secrets[key] = defaultValue;
                if (secrets[key] != null)
                {
                    log.WriteWarning(nameof(ConfigurationBuilderExtensions), nameof(AddEnvironmentSecrets), 
                        $"Value of {key} satisfied by default value");
                }
                else
                {
                    log.WriteError(nameof(ConfigurationBuilderExtensions), nameof(AddEnvironmentSecrets),
                        new Exception($"Cannot find a value in secrets.json for {key} or a value for the environment variable named {environmentVariable} and no default value was specified."));
                }
            }

            return builder.AddInMemoryCollection(secrets);
        }
    }
}
