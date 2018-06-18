using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.SlackNotifications;
using MarginTrading.AccountsManagement.BrokerBase.Settings;

namespace MarginTrading.AccountsManagement.BrokerBase
{
    public abstract class BrokerApplicationBase<TMessage> : IBrokerApplication
    {
        protected readonly ILog _logger;
        private readonly ISlackNotificationsSender _slackNotificationsSender;
        protected readonly CurrentApplicationInfo ApplicationInfo;
        private RabbitMqSubscriber<TMessage> _connector;

        protected abstract BrokerSettingsBase Settings { get; }
        protected abstract string ExchangeName { get; }
        protected abstract string RoutingKey { get; }

        protected virtual string QueuePostfix => string.Empty;
        
        private RabbitMqSubscriptionSettings GetRabbitMqSubscriptionSettings()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = Settings.MtRabbitMqConnString,
                QueueName = RabbitExtensions.BuildQueueName(ExchangeName, Settings.Env, QueuePostfix),
                ExchangeName = ExchangeName,
                IsDurable = true
            };
            if (!string.IsNullOrWhiteSpace(RoutingKey))
            {
                settings.RoutingKey = RoutingKey;
            }
            return settings;
        }

        protected abstract Task HandleMessage(TMessage accountChangedEvent);

        protected BrokerApplicationBase(ILog logger, ISlackNotificationsSender slackNotificationsSender,
            CurrentApplicationInfo applicationInfo)
        {
            _logger = logger;
            _slackNotificationsSender = slackNotificationsSender;
            ApplicationInfo = applicationInfo;
        }

        public virtual void Run()
        {
            WriteInfoToLogAndSlack("Starting listening exchange " + ExchangeName);
            
            try
            {
                var settings = GetRabbitMqSubscriptionSettings();
                _connector =
                    new RabbitMqSubscriber<TMessage>(settings,
                            new ResilientErrorHandlingStrategy(_logger, settings, TimeSpan.FromSeconds(1)))
                        .SetMessageDeserializer(new MessagePackMessageDeserializer<TMessage>())
                        .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                        .Subscribe(HandleMessage)
                        .SetLogger(_logger)
                        .Start();

                WriteInfoToLogAndSlack("Broker listening queue " + settings.QueueName);
            }
            catch (Exception ex)
            {
                _logger.WriteErrorAsync(ApplicationInfo.ApplicationFullName, "Application.RunAsync", null, ex).GetAwaiter()
                    .GetResult();
            }
        }

        public void StopApplication()
        {
            Console.WriteLine($"Closing {ApplicationInfo.ApplicationFullName}...");
            WriteInfoToLogAndSlack("Stopping listening exchange " + ExchangeName);
            _connector.Stop();
        }

        protected void WriteInfoToLogAndSlack(string infoMessage)
        {
            _logger.WriteInfoAsync(ApplicationInfo.ApplicationFullName, null, null, infoMessage);
            _slackNotificationsSender?.SendAsync("MarginTrading", ApplicationInfo.ApplicationFullName,
                infoMessage);
        }
    }
}
