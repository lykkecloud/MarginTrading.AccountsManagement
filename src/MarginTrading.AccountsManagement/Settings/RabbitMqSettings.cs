using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class RabbitMqSettings
    {
        public RabbitConnectionSettings NegativeProtection { get; set; }
    }
}