using Microsoft.Extensions.Configuration;

namespace MarginTrading.TradingHistory.Extensions
{
    public static class ConfigurationRootExtensions
    {
        public static bool NotThrowExceptionsOnServiceValidation(this IConfigurationRoot configuration)
        {
            return !string.IsNullOrEmpty(configuration["NOT_THROW_EXCEPTIONS_ON_SERVICES_VALIDATION"]) &&
                   bool.TryParse(configuration["NOT_THROW_EXCEPTIONS_ON_SERVICES_VALIDATION"],
                       out var trowExceptionsOnInvalidService) && trowExceptionsOnInvalidService;
        }
    }
}