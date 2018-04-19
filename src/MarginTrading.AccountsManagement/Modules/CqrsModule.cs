using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.Saga;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.TradingEngineMock;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;
using MarginTrading.AccountsManagement.Workflow.Commands;
using MarginTrading.AccountsManagement.Workflow.Sagas;

namespace MarginTrading.AccountsManagement.Modules
{
    public class CqrsModule : Module
    {
        private const string DefaultRoute = "self";
        private const string DefaultPipeline = "commands";
        private readonly CqrsSettings _settings;
        private readonly ILog _log;
        private readonly long _defaultRetryDelayMs;

        public CqrsModule(CqrsSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _defaultRetryDelayMs = (long) _settings.RetryDelay.TotalMilliseconds;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.ConnectionString
            };
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory());

            // Sagas & command handlers
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => t.Name.EndsWith("Saga") || t.Name.EndsWith("CommandsHandler"))
                .AsSelf();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var rabbitMqConventionEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                "messagepack",
                environment: _settings.EnvironmentName);
            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(rabbitMqConventionEndpointResolver),
                RegisterWithdrawalSaga(),
                RegisterContext(),
                RegisterMockTradingEngineEContext());
        }

        private IRegistration RegisterWithdrawalSaga()
        {
            var sagaRegistration = RegisterSaga<WithdrawalFailedEvent>();
            
            sagaRegistration
                .ListeningEvents(typeof(WithdrawalStartedEvent), typeof(AccountBalanceChangedEvent))
                .From(_settings.ContextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(FreezeAmountForWithdrawalCommand))
                .To(_settings.ContextNames.TradingEngine)
                .With(DefaultPipeline);
            
            sagaRegistration
                .ListeningEvents(typeof(AmountForWithdrawalFrozenEvent), typeof(AmountForWithdrawalFreezeFailedEvent))
                .From(_settings.ContextNames.TradingEngine)
                .On(DefaultRoute)
                .PublishingCommands(typeof(UpdateBalanceInternalCommand), typeof(FailWithdrawalInternalCommand))
                .To(_settings.ContextNames.AccountsManagement)
                .With(DefaultPipeline);

            return sagaRegistration;
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_settings.ContextNames.AccountsManagement)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024);

            contextRegistration.ListeningCommands(typeof(BeginWithdrawalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<WithdrawalSaga>()
                .PublishingEvents(typeof(WithdrawalFailedEvent), typeof(WithdrawalStartedEvent))
                .With(DefaultPipeline);

            contextRegistration.ListeningCommands(typeof(FailWithdrawalInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<WithdrawalSaga>()
                .PublishingEvents(typeof(WithdrawalFailedEvent))
                .With(DefaultPipeline);

            contextRegistration.ListeningCommands(typeof(CompleteWithdrawalInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<WithdrawalSaga>()
                .PublishingEvents(typeof(WithdrawalCompletedEvent))
                .With(DefaultPipeline);

            return contextRegistration;
        }

        private IRegistration RegisterMockTradingEngineEContext()
        {
            var contextRegistration = Register.BoundedContext(_settings.ContextNames.TradingEngine)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024);

            contextRegistration.ListeningCommands(typeof(FreezeAmountForWithdrawalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<FreezeAmountForWithdrawalCommandHandler>()
                .PublishingEvents(typeof(AmountForWithdrawalFrozenEvent), typeof(AmountForWithdrawalFreezeFailedEvent))
                .With(DefaultPipeline);

            return contextRegistration;
        }

        private ISagaRegistration RegisterSaga<TSaga>()
        {
            return Register.Saga<TSaga>($"{_settings.ContextNames.AccountsManagement}.{nameof(TSaga)}");
        }
    }
}