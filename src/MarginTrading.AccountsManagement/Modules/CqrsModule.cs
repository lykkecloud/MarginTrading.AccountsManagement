using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Cqrs.Configuration.Routing;
using Lykke.Cqrs.Configuration.Saga;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.TradingEngineMock;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;
using MarginTrading.AccountsManagement.Workflow.Deposit;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;

namespace MarginTrading.AccountsManagement.Modules
{
    internal class CqrsModule : Module
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
                RegisterDefaultRouting(),
                RegisterWithdrawalSaga(),
                RegisterUpdateBalanceSaga(),
                RegisterDepositSaga(),
                RegisterContext(),
                RegisterMockTradingEngineContext());
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_settings.ContextNames.AccountsManagement)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024);
            RegisterWithdrawalCommandHandler(contextRegistration);
            RegisterDepositCommandHandler(contextRegistration);
            RegisterUpdateBalanceCommandHandler(contextRegistration);
            return contextRegistration;
        }

        private PublishingCommandsDescriptor<IDefaultRoutingRegistration> RegisterDefaultRouting()
        {
            return Register.DefaultRouting
                .PublishingCommands(
                    typeof(BeginBalanceUpdateInternalCommand), 
                    typeof(BeginWithdrawalCommand),
                    typeof(BeginDepositCommand))
                .To(_settings.ContextNames.AccountsManagement).With(DefaultPipeline);
        }

        private IRegistration RegisterWithdrawalSaga()
        {
            var sagaRegistration = RegisterSaga<WithdrawalSaga>();

            sagaRegistration
                .ListeningEvents(typeof(WithdrawalStartedEvent))
                .From(_settings.ContextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(FreezeAmountForWithdrawalCommand))
                .To(_settings.ContextNames.TradingEngine)
                .With(DefaultPipeline);

            sagaRegistration
                .ListeningEvents(typeof(AmountForWithdrawalFrozenEvent), typeof(AmountForWithdrawalFreezeFailedEvent))
                .From(_settings.ContextNames.TradingEngine)
                .On(DefaultRoute)
                .PublishingCommands(typeof(BeginBalanceUpdateInternalCommand), typeof(FailWithdrawalInternalCommand))
                .To(_settings.ContextNames.AccountsManagement)
                .With(DefaultPipeline);
            
            sagaRegistration
                .ListeningEvents(typeof(AccountBalanceChangedEvent))
                .From(_settings.ContextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(CompleteWithdrawalInternalCommand))
                .To(_settings.ContextNames.AccountsManagement)
                .With(DefaultPipeline);

            return sagaRegistration;
        }

        private static void RegisterWithdrawalCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration.ListeningCommands(
                    typeof(BeginWithdrawalCommand),
                    typeof(FailWithdrawalInternalCommand),
                    typeof(CompleteWithdrawalInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<WithdrawalCommandsHandler>()
                .PublishingEvents(
                    typeof(WithdrawalFailedEvent),
                    typeof(WithdrawalStartedEvent),
                    typeof(WithdrawalCompletedEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterDepositSaga()
        {
            return RegisterSaga<DepositSaga>()
                .ListeningEvents(
                    typeof(DepositStartedEvent),
                    typeof(AccountBalanceChangedEvent),
                    typeof(AmountForDepositFrozenEvent),
                    typeof(AmountForDepositFreezeFailedEvent))
                .From(_settings.ContextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(
                    typeof(FreezeAmountForDepositInternalCommand),
                    typeof(BeginBalanceUpdateInternalCommand),
                    typeof(FailDepositInternalCommand),
                    typeof(CompleteDepositInternalCommand))
                .To(_settings.ContextNames.AccountsManagement)
                .With(DefaultPipeline);
        }

        private static void RegisterDepositCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(BeginDepositCommand),
                    typeof(FreezeAmountForDepositInternalCommand),
                    typeof(FailDepositInternalCommand),
                    typeof(CompleteDepositInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<DepositCommandsHandler>()
                .PublishingEvents(
                    typeof(DepositFailedEvent),
                    typeof(AmountForDepositFrozenEvent),
                    typeof(DepositStartedEvent),
                    typeof(DepositCompletedEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterUpdateBalanceSaga()
        {
            return RegisterSaga<UpdateBalanceSaga>()
                .ListeningEvents(typeof(AccountBalanceUpdateStartedEvent))
                .From(_settings.ContextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(UpdateBalanceInternalCommand))
                .To(_settings.ContextNames.AccountsManagement)
                .With(DefaultPipeline);
        }

        private static void RegisterUpdateBalanceCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(BeginBalanceUpdateInternalCommand),
                    typeof(UpdateBalanceInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<UpdateBalanceCommandsHandler>()
                .PublishingEvents(
                    typeof(AccountBalanceUpdateStartedEvent), 
                    typeof(AccountBalanceChangedEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterMockTradingEngineContext()
        {
            return Register.BoundedContext(_settings.ContextNames.TradingEngine)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024)
                .ListeningCommands(typeof(FreezeAmountForWithdrawalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<FreezeAmountForWithdrawalCommandsHandler>()
                .PublishingEvents(
                    typeof(AmountForWithdrawalFrozenEvent),
                    typeof(AmountForWithdrawalFreezeFailedEvent))
                .With(DefaultPipeline);
        }

        private ISagaRegistration RegisterSaga<TSaga>()
        {
            return Register.Saga<TSaga>($"{_settings.ContextNames.AccountsManagement}.{typeof(TSaga).Name}");
        }
    }
}