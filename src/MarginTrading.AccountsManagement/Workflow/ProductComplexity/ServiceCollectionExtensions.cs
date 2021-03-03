using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.Common.Startup;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.Backend.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarginTrading.AccountsManagement.Workflow.ProductComplexity
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddProductComplexity(this IServiceCollection services, AppSettings settings)
        {
            services.AddHostedService<CleanupExpiredComplexityService>();
            services.AddHostedService<OrderHistoryListener>();
            
            services.AddSingleton(ctx => new RabbitMqSubscriber<OrderHistoryEvent>(
                    settings.MarginTradingAccountManagement.RabbitMq.OrderHistory,
                    new DefaultErrorHandlingStrategy(
                        new LykkeLoggerAdapter<RabbitMqSubscriber<OrderHistoryEvent>>(
                            ctx.GetRequiredService<ILogger<RabbitMqSubscriber<OrderHistoryEvent>>>()),
                        settings.MarginTradingAccountManagement.RabbitMq.OrderHistory))
                .SetMessageDeserializer(new JsonMessageDeserializer<OrderHistoryEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .SetLogger(new LykkeLoggerAdapter<RabbitMqSubscriber<OrderHistoryEvent>>(
                    ctx.GetRequiredService<ILogger<RabbitMqSubscriber<OrderHistoryEvent>>>()))
                .CreateDefaultBinding());
        }
    }
}
