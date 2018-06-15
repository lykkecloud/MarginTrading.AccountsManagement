using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.AccountsManagement.BrokerBase
{
    public static class RabbitExtensions
    {
        public static string BuildQueueName(string exchangeName, string env, string postfix = "")
        {
            return
                $"{exchangeName}.{PlatformServices.Default.Application.ApplicationName}.{env ?? "DefaultEnv"}{postfix}";
        }
    }
}