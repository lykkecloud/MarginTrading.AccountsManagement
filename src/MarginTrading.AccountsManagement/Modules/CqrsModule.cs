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
using MarginTrading.AccountsManagement.Workflow.ClosePosition;
using MarginTrading.AccountsManagement.Workflow.Deposit;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.Deposit.Events;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;

namespace MarginTrading.AccountsManagement.Modules
{
    internal class CqrsModule : Module
    {
        private const string DefaultRoute = "self";
        private const string DefaultPipeline = "commands";
        private readonly CqrsSettings _settings;
        private readonly ILog _log;
        private readonly long _defaultRetryDelayMs;
        private readonly CqrsContextNamesSettings _contextNames;

        public CqrsModule(CqrsSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _defaultRetryDelayMs = (long) _settings.RetryDelay.TotalMilliseconds;
            _contextNames = _settings.ContextNames;
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
                RegisterDepositSaga(),
                RegisterClosePositionSaga(),
                RegisterContext());
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_contextNames.AccountsManagement)
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
                    typeof(UpdateBalanceInternalCommand), 
                    typeof(WithdrawCommand),
                    typeof(DepositCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);
        }

        private IRegistration RegisterWithdrawalSaga()
        {
            var sagaRegistration = RegisterSaga<WithdrawalSaga>();

            sagaRegistration
                .ListeningEvents(typeof(WithdrawalStartedInternalEvent))
                .From(_contextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(FreezeAmountForWithdrawalCommand))
                .To(_contextNames.TradingEngine)
                .With(DefaultPipeline);

            sagaRegistration
                .ListeningEvents(typeof(AmountForWithdrawalFrozenEvent), typeof(AmountForWithdrawalFreezeFailedEvent))
                .From(_contextNames.TradingEngine)
                .On(DefaultRoute)
                .PublishingCommands(typeof(UpdateBalanceInternalCommand), typeof(FailWithdrawalInternalCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);
            
            sagaRegistration
                .ListeningEvents(typeof(AccountBalanceChangedEvent), typeof(AccountBalanceChangeFailedEvent))
                .From(_contextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(typeof(UnfreezeMarginWithdrawalCommand), typeof(UnfreezeMarginOnFailWithdrawalCommand))
                .To(_contextNames.TradingEngine)
                .With(DefaultPipeline);
            
            sagaRegistration
                .ListeningEvents(typeof(UnfreezeMarginSucceededWithdrawalEvent), typeof(UnfreezeMarginOnFailSucceededWithdrawalEvent))
                .From(_contextNames.TradingEngine)
                .On(DefaultRoute)
                .PublishingCommands(typeof(CompleteWithdrawalInternalCommand), typeof(FailWithdrawalInternalCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);

            sagaRegistration
                .ListeningEvents(typeof(WithdrawalFailedEvent), typeof(WithdrawalSucceededEvent))
                .From(_contextNames.AccountsManagement)
                .On(DefaultRoute);

            return sagaRegistration;
        }

        private static void RegisterWithdrawalCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration.ListeningCommands(
                    typeof(WithdrawCommand),
                    typeof(FailWithdrawalInternalCommand),
                    typeof(CompleteWithdrawalInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<WithdrawalCommandsHandler>()
                .PublishingEvents(
                    typeof(WithdrawalFailedEvent),
                    typeof(WithdrawalStartedInternalEvent),
                    typeof(WithdrawalSucceededEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterDepositSaga()
        {
            return RegisterSaga<DepositSaga>()
                .ListeningEvents(
                    typeof(DepositStartedInternalEvent),
                    typeof(AccountBalanceChangedEvent), 
                    typeof(AccountBalanceChangeFailedEvent),
                    typeof(AmountForDepositFrozenInternalEvent),
                    typeof(AmountForDepositFreezeFailedInternalEvent))
                .From(_contextNames.AccountsManagement)
                .On(DefaultRoute)
                .PublishingCommands(
                    typeof(FreezeAmountForDepositInternalCommand),
                    typeof(UpdateBalanceInternalCommand),
                    typeof(FailDepositInternalCommand),
                    typeof(CompleteDepositInternalCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);
        }

        private static void RegisterDepositCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(DepositCommand),
                    typeof(FreezeAmountForDepositInternalCommand),
                    typeof(FailDepositInternalCommand),
                    typeof(CompleteDepositInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<DepositCommandsHandler>()
                .PublishingEvents(
                    typeof(DepositFailedEvent),
                    typeof(AmountForDepositFrozenInternalEvent),
                    typeof(DepositStartedInternalEvent),
                    typeof(DepositSucceededEvent))
                .With(DefaultPipeline);
        }

        private static void RegisterUpdateBalanceCommandHandler(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration
                .ListeningCommands(
                    typeof(UpdateBalanceInternalCommand))
                .On(DefaultRoute)
                .WithCommandsHandler<UpdateBalanceCommandsHandler>()
                .PublishingEvents(
                    typeof(AccountBalanceChangedEvent),
                    typeof(AccountChangedEvent),
                    typeof(AccountBalanceChangeFailedEvent))
                .With(DefaultPipeline);
        }

        private IRegistration RegisterClosePositionSaga()
        {
            return RegisterSaga<ClosePositionSaga>()
                .ListeningEvents(
                    typeof(Backend.Contracts.Events.PositionClosedEvent))
                .From(_contextNames.TradingEngine)
                .On(DefaultRoute)
                .PublishingCommands(
                    typeof(UpdateBalanceInternalCommand))
                .To(_contextNames.AccountsManagement)
                .With(DefaultPipeline);
        }

        private ISagaRegistration RegisterSaga<TSaga>()
        {
            return Register.Saga<TSaga>($"{_contextNames.AccountsManagement}.{typeof(TSaga).Name}");
        }
    }
}