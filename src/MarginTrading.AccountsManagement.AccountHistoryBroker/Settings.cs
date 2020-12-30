// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    [UsedImplicitly]
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        
        public RabbitMqQueues RabbitMqQueues { get; set; }

        public ServiceSettings AccountManagement { get; set; }
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

    public class ServiceSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }

        [Optional]
        public string ApiKey { get; set; }
    }
}