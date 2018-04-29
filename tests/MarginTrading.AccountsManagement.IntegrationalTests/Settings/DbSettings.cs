using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.IntegrationalTests.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        
        public string ConnectionString { get; set; }
        public string LogsConnString { get; set; }
    }
}
