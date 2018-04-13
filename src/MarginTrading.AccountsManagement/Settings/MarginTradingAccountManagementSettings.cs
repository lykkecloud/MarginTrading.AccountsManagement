namespace MarginTrading.AccountsManagement.Settings
{
    internal class MarginTradingAccountManagementSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
    }
}
