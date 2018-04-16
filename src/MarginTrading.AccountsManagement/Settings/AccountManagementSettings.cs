using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class AccountManagementSettings
    {
        /// <summary>
        /// DB connection strings
        /// </summary>
        public DbSettings Db { get; set; }
        
        /// <summary>
        /// RabbitMq exchanges connections
        /// </summary>
        public RabbitMqSettings RabbitMq { get; set; }
        
        /// <summary>
        /// Behavior settings for accounts
        /// </summary>
        [Optional, CanBeNull]
        public BehaviorSettings Behavior { get; set; }
        
    }
}
