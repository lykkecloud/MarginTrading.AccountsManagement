using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;
using MarginTrading.AccountsManagement.Workflow.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Sagas
{
    internal class WithdrawalSaga
    {
        private const string OperationName = "Withdraral";

        private readonly IConvertService _convertService;
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IOperationStatesRepository _operationStatesRepository;

        public WithdrawalSaga(IConvertService convertService, CqrsContextNamesSettings contextNames,
            IAccountManagementService accountManagementService, IOperationStatesRepository operationStatesRepository)
        {
            _convertService = convertService;
            _contextNames = contextNames;
            _accountManagementService = accountManagementService;
            _operationStatesRepository = operationStatesRepository;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(BeginWithdrawalCommand command, IEventPublisher publisher)
        {
            var existed = await _operationStatesRepository.Get(OperationName, command.OperationId) != null;
            if (existed)
            {
                return;
            }

            var account = await _accountManagementService.GetByClientAndIdAsync(command.ClientId, command.AccountId);
            if (account == null || account.IsDisabled)
            {
                publisher.PublishEvent(new WithdrawalFailedEvent(command.ClientId, command.AccountId,
                    command.Amount, command.OperationId,
                    account == null ? "Account does not exist" : "Account is disabled"));
                await _operationStatesRepository.InsertIfNotExistsAsync(new OperationState(OperationName,
                    command.OperationId,
                    States.Failed.ToString()));
            }
            else
            {
                publisher.PublishEvent(_convertService.Convert<WithdrawalStartedEvent>(command));
                await _operationStatesRepository.InsertIfNotExistsAsync(new OperationState(OperationName,
                    command.OperationId,
                    States.Received.ToString()));
            }
        }

        /// <summary>
        /// The withdrawal has started. Ask the backend to freeze the the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private Task Handle(WithdrawalStartedEvent evt, ICommandSender sender)
        {
            return _operationStatesRepository.TryChangeState(OperationName, evt.OperationId,
                States.Received, () =>
                {
                    sender.SendCommand(_convertService.Convert<FreezeAmountForWithdrawalCommand>(evt),
                        _contextNames.TradingEngine);
                    return States.FreezeAmountCommandSent;
                });
        }

        /// <summary>
        /// The backend frozen the the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForWithdrawalFrozenEvent evt, ICommandSender sender)
        {
            return _operationStatesRepository.TryChangeState(OperationName, evt.OperationId,
                States.FreezeAmountCommandSent, () =>
                {
                    sender.SendCommand(_convertService.Convert<UpdateBalanceInternalCommand>(evt),
                        _contextNames.AccountsManagement);
                    return States.AmountFrozen;
                });
        }

        /// <summary>
        /// The backend failed to freeze the the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForWithdrawalFreezeFailedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(new FailWithdrawalInternalCommand(evt.ClientId, evt.AccountId, evt.Amount, 
                    evt.OperationId, "Backend failed to freeze amount: " + evt.Reason),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new WithdrawalFailedEvent(command.ClientId, command.AccountId,
                command.Amount, command.OperationId, command.Reason));
            return _operationStatesRepository.ChangeState(OperationName, command.OperationId, States.Failed);
        }

        /// <summary>
        /// The balance has changed, finish the operation
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AccountBalanceChangedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(_convertService.Convert<CompleteWithdrawalInternalCommand>(evt),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new WithdrawalCompletedEvent(command.ClientId, command.AccountId,
                command.Amount, command.OperationId, command.Reason));
            return _operationStatesRepository.ChangeState(OperationName, command.OperationId, States.Finished);
        }

        private enum States
        {
            Received = 1,
            FreezeAmountCommandSent = 2,
            AmountFrozen = 3,
            Finished = 4,
            Failed = 5,
        }
    }
}