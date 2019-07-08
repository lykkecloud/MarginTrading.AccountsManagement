// Copyright (c) 2019 Lykke Corp.

using System;
using System.Threading.Tasks;
using Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Infrastructure
{
    public interface IRabbitMqService
    {
        IMessageProducer<TMessage> GetProducer<TMessage>(IReloadingManager<RabbitConnectionSettings> settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer);

        void Subscribe<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable,
            Func<TMessage, Task> handler);

        IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>();
        IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>();
    }
}