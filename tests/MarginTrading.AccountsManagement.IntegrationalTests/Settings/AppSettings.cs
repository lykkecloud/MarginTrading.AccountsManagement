using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.IntegrationalTests.Settings
{
    [UsedImplicitly]
    internal class AppSettings
    {
        public AccountManagementSettings MarginTradingAccountManagement { get; set; }
        public AccountManagementServiceClientSettings MarginTradingAccountManagementServiceClient { get; set; }
    }
}
