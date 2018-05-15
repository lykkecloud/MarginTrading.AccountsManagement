using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    internal class AppSettings
    {
        public AccountManagementSettings MarginTradingAccountManagement { get; set; }
        public MarginTradingSettingsServiceClientSettings MarginTradingSettingsServiceClient { get; set; }
    }
}
