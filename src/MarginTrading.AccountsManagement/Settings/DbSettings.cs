using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        public string ConnectionString { get; set; }
        public string LogsConnString { get; set; }
        
        public string SqlConnectionString { get; set; }
    }
}
