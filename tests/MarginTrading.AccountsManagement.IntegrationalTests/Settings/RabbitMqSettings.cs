using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.IntegrationalTests.Settings
{
    [UsedImplicitly]
    public class RabbitMqSettings
    {
        public RabbitConnectionSettings AccountChangedExchange { get; set; }
        public RabbitConnectionSettings AccountHistoryExchange { get; set; }
    }
}