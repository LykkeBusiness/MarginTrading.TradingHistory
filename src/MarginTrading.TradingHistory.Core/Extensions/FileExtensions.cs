using System;
using System.IO;

namespace MarginTrading.TradingHistory.Core.Extensions
{
    public static class FileExtensions
    {
        public static string ReadFromFile(string scriptFileName)
        {
            // TODO: Should be exposed via settings
            var debugLocation = $"{Directory.GetCurrentDirectory()}/../../Scripts/{scriptFileName}";
            var prodLocation = $"./Scripts/{scriptFileName}";

            var fileContent = GetFileContent(prodLocation) ?? GetFileContent(debugLocation);
            if (fileContent == null)
            {
                throw new Exception($"Both prod and debug locations contain no [{scriptFileName}] file. DEBUG: [{debugLocation}]. PROD: [{prodLocation}]");
            }

            return fileContent;
        }

        private static string GetFileContent(string filePath) =>
            File.Exists(filePath) ? File.ReadAllText(filePath) : null;
    }
}
