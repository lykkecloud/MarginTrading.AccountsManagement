using Microsoft.Extensions.Configuration;

namespace MarginTrading.AccountsManagement.Extensions
{
    public static class ConfigurationRootExtensions
    {
        public static bool NotTrowExceptionsOnServiceValidation(this IConfigurationRoot configuration)
        {
            return !string.IsNullOrEmpty(configuration["NOT_TROW_EXCEPTIONS_ON_SERVICES_VALIDATION"]) &&
                   bool.TryParse(configuration["NOT_TROW_EXCEPTIONS_ON_SERVICES_VALIDATION"],
                       out var trowExceptionsOnInvalidService) && trowExceptionsOnInvalidService;
        }
    }
}