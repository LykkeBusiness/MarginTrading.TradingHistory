using System;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using MarginTrading.TradingHistory.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MarginTrading.TradingHistory
{
    public static class SecurityHelpers
    {
        [CanBeNull]
        private static X509Certificate2 LoadCertificate(string path, string pwd)
        {
            if (path != null && pwd != null)
            {
                return new X509Certificate2(path, pwd);
            }

            return null;
        }

        public static void ConfigureHttpsEndpoint(ListenOptions listenOptions)
        {
            var certificate = LoadCertificate(
                Environment.GetEnvironmentVariable("X509_CERTIFICATE_PATH"), 
                Environment.GetEnvironmentVariable("X509_CERTIFICATE_PASSWORD"));
            if (certificate != null)
            {
                listenOptions.UseHttps(certificate);
            }
            else
            {
                listenOptions.UseHttps();
            }
        }
    }
}
