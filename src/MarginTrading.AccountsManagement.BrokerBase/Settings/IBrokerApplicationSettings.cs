using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public interface IBrokerApplicationSettings<TBrokerSettings> 
        where TBrokerSettings : BrokerSettingsBase
    {
        [Optional, CanBeNull]
        SlackNotificationSettings SlackNotifications { get; }
        
        [Optional, CanBeNull]
        BrokersLogsSettings MtBrokersLogs { get; set; }
        
        BrokerSettingsRoot<TBrokerSettings> MtBackend { get; set; }
    }
}
