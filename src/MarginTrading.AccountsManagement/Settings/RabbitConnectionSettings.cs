using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class RabbitConnectionSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
    }
}
