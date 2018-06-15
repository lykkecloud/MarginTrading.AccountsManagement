using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public class DefaultBrokerApplicationSettings<TBrokerSettings>: IBrokerApplicationSettings<TBrokerSettings>
        where TBrokerSettings: BrokerSettingsBase
    {
        [Optional, CanBeNull]
        public SlackNotificationSettings SlackNotifications { get; set; }
        
        [Optional, CanBeNull]
        public BrokersLogsSettings MtBrokersLogs { get; set; }
        
        public BrokerSettingsRoot<TBrokerSettings> MtBackend { get; set; }
    }
}
