using JetBrains.Annotations;
using Lykke.Common.Chaos;
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
        
        public CqrsSettings Cqrs { get; set; }
        
        [Optional]
        public bool EnableOperationsLogs { get; set; }
        
        [Optional]
        public bool NegativeProtectionAutoCompensation { get; set; }

        [Optional, CanBeNull]
        public ChaosSettings ChaosKitty { get; set; }
        
        [Optional]
        public bool UseSerilog { get; set; }
    }
}
