using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public class BrokerSettingsRoot<TBrokerSettings>
        where TBrokerSettings: BrokerSettingsBase
    {
        public TBrokerSettings MarginTradingLive { get; set; }
        [Optional, CanBeNull]
        public TBrokerSettings MarginTradingDemo { get; set; }
    }
}
