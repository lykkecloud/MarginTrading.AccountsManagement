using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    internal class AppSettings
    {
        public AccountManagementSettings MarginTradingAccountManagement { get; set; }
        
        public ClientSettings MarginTradingSettingsServiceClient { get; set; }
        public ClientSettings MtBackendServiceClient { get; set; }
    }
}
