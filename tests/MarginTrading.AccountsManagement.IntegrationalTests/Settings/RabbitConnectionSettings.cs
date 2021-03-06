﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.IntegrationalTests.Settings
{
    [UsedImplicitly]
    public class RabbitConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string ExchangeName { get; set; }
        
        [Optional, CanBeNull] 
        public string RoutingKey { get; set; }
    }
}