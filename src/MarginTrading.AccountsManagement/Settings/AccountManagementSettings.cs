// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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

        /// <summary>
        /// Shows if negative account capital will be automatically compensated.
        /// Enabled by default.
        /// </summary>
        [Optional] 
        public bool NegativeProtectionAutoCompensation { get; set; } = true;

        [Optional]
        public int NegativeProtectionTimeoutMs { get; set; } = 30000;

        [Optional, CanBeNull]
        public ChaosSettings ChaosKitty { get; set; }
        
        [Optional]
        public bool UseSerilog { get; set; }
    }
}
