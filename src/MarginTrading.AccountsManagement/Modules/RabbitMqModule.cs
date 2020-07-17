// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using BookKeeper.Client.Workflow.Events;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Services.Implementation;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.Backend.Contracts.TradingSchedule;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.AccountsManagement.Modules
{
    internal class RabbitMqModule : Module
    {
        private readonly ILog _log;
        private readonly SubscriberSettings _marketStateChangedSettings;
        private readonly SubscriberSettings _taxFileUploadedSettings;

        public RabbitMqModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _marketStateChangedSettings = settings.CurrentValue.MarginTradingAccountManagement.RabbitMq.MarketStateChangedEventSubscriber;
            _taxFileUploadedSettings = settings.CurrentValue.MarginTradingAccountManagement.RabbitMq.TaxFileUploadedEventSubscriber;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            #region MarketStateChangedEvent
            
            var marketStateRabbitMqSettings = _marketStateChangedSettings.ToRabbitMqSettings();

            builder.Register(ctx => new RabbitMqSubscriber<MarketStateChangedEvent>(
                        marketStateRabbitMqSettings,
                        new DefaultErrorHandlingStrategy(_log, marketStateRabbitMqSettings))
                    .SetMessageDeserializer(new MessagePackMessageDeserializer<MarketStateChangedEvent>())
                    .SetMessageReadStrategy(new MessageReadQueueStrategy())
                    .SetLogger(_log)
                    .CreateDefaultBinding())
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterType<MarketStateChangedEventHandler>()
                .As<IHostedService>()
                .SingleInstance();
            
            #endregion
            
            #region TaxFileUploadedEvent
            
            var taxFileUploadedRabbitMqSettings = _taxFileUploadedSettings.ToRabbitMqSettings();
            
            builder.Register(ctx => new RabbitMqSubscriber<TaxFileUploadedEvent>(
                        taxFileUploadedRabbitMqSettings,
                        new DefaultErrorHandlingStrategy(_log, taxFileUploadedRabbitMqSettings))
                    .SetMessageDeserializer(new MessagePackMessageDeserializer<TaxFileUploadedEvent>())
                    .SetMessageReadStrategy(new MessageReadQueueStrategy())
                    .SetLogger(_log)
                    .CreateDefaultBinding())
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterType<TaxFileUploadedEventHandler>()
                .As<IHostedService>()
                .SingleInstance();
            
            #endregion
        }
    }
}