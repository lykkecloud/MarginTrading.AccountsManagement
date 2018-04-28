using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class RabbitMqSettings
    {
        public RabbitConnectionSettings AccountChangedExchange { get; set; }
        public RabbitConnectionSettings AccountHistoryExchange { get; set; }
    }
}