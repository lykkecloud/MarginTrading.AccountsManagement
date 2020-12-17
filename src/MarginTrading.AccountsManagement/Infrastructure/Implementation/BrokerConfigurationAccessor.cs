namespace MarginTrading.AccountsManagement.Infrastructure.Implementation
{
    public class BrokerConfigurationAccessor
    {
        public BrokerConfigurationAccessor(string brokerId)
        {
            BrokerId = brokerId;
        }

        public string BrokerId { get; }
    }
}
