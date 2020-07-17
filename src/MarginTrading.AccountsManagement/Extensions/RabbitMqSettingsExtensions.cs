// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Extensions
{
    public static class RabbitMqSettingsExtensions
    {
        public static RabbitMqSubscriptionSettings ToRabbitMqSettings(this SubscriberSettings settings, bool isDurable = true)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            
            return new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.ExchangeName,
                QueueName = settings.QueueName,
                RoutingKey = settings.RoutingKey,
                IsDurable = isDurable
            };
        }
    }
}