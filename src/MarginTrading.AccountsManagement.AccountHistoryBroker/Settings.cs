// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase.Settings;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    [UsedImplicitly]
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        
        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
    
    [UsedImplicitly]
    public class Db
    {
        public string StorageMode { get; set; }
        
        public string ConnString { get; set; }
    }
    
    [UsedImplicitly]
    public class RabbitMqQueues
    {
        public RabbitMqQueueInfo AccountHistory { get; set; }
    }

    [UsedImplicitly]
    public class RabbitMqQueueInfo
    {
        public string ExchangeName { get; set; }
    }
}