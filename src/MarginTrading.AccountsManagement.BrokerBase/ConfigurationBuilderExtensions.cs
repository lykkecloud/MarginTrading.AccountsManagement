using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MarginTrading.AccountsManagement.BrokerBase
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
    }
}