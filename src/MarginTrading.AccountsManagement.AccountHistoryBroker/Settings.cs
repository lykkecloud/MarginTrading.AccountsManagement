using Lykke.MarginTrading.BrokerBase.Settings;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
    
    public class Db
    {
        public string StorageMode { get; set; }
        public string HistoryConnString { get; set; }
        public string LogsConnString { get; set; }
    }
    
    public class RabbitMqQueues
    {
        public RabbitMqQueueInfo AccountHistory { get; set; }
    }

    public class RabbitMqQueueInfo
    {
        public string ExchangeName { get; set; }
    }
}