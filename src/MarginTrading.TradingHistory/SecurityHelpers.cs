using System;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace MarginTrading.TradingHistory
{
    public static class SecurityHelpers
    {
        public static void ConfigureHttpsEndpoint(ListenOptions listenOptions, IConfiguration configuration)
        {
            var certificate = LoadCertificate(
                configuration.GetValue<string>("X509-Certificate-Path"),
                configuration.GetValue<string>("X509-Certificate-Password"));
            if (certificate != null)
            {
                listenOptions.UseHttps(certificate);
            }
            else
            {
                listenOptions.UseHttps();
            }
        }
        
        [CanBeNull]
        private static X509Certificate2 LoadCertificate(string path, string pwd)
        {
            if (path != null && pwd != null)
            {
                return new X509Certificate2(path, pwd);
            }

            return null;
        }
    }
}
