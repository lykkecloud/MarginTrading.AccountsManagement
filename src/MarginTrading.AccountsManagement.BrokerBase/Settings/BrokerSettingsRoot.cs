using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public class BrokerSettingsRoot<TBrokerSettings>
        where TBrokerSettings: BrokerSettingsBase
    {
        public TBrokerSettings MarginTradingSettings { get; set; }
    }
}
