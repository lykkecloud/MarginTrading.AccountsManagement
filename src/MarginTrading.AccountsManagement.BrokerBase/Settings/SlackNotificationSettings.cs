using Lykke.AzureQueueIntegration;

namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public class SlackNotificationSettings
    {
        public AzureQueueSettings AzureQueue { get; set; }
    }
}
