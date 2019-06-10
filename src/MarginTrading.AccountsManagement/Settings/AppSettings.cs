using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    internal class AppSettings
    {
        public AccountManagementSettings MarginTradingAccountManagement { get; set; }

        [Optional]
        public Lykke.Snow.Common.Startup.ApiKey.ClientSettings MarginTradingAccountManagementServiceClient { get; set; }
         = new Lykke.Snow.Common.Startup.ApiKey.ClientSettings();
        
        public ClientSettings MarginTradingSettingsServiceClient { get; set; }
        
        public OptionalClientSettings MtBackendServiceClient { get; set; }
    }
}
