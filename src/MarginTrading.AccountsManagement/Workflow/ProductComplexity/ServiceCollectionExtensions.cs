using System;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs;
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
                    BuildErrorHandlingStrategy(ctx, settings.MarginTradingAccountManagement.RabbitMq.OrderHistory))
                .SetMessageDeserializer(new JsonMessageDeserializer<OrderHistoryEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .SetLogger(new LykkeLoggerAdapter<RabbitMqSubscriber<OrderHistoryEvent>>(
                    ctx.GetRequiredService<ILogger<RabbitMqSubscriber<OrderHistoryEvent>>>()))
                .CreateDefaultBinding());
        }

        private static IErrorHandlingStrategy BuildErrorHandlingStrategy(IServiceProvider provider, SubscriptionSettings settings)
        {
            var logger = new LykkeLoggerAdapter<RabbitMqSubscriber<OrderHistoryEvent>>(provider.GetRequiredService<ILogger<RabbitMqSubscriber<OrderHistoryEvent>>>());
            var dlqStrategy = new DeadQueueErrorHandlingStrategy(logger, settings);
            
            return new ResilientErrorHandlingStrategy(logger, settings, TimeSpan.FromSeconds(1), next: dlqStrategy);
        }
    }
}
