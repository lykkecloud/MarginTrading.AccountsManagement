using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalCommandsHandler
    {
        private const string OperationName = "Withdrawal";
        private readonly IOperationStatesRepository _operationStatesRepository;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IConvertService _convertService;

        public WithdrawalCommandsHandler(IOperationStatesRepository operationStatesRepository,
            IAccountManagementService accountManagementService, IConvertService convertService)
        {
            _operationStatesRepository = operationStatesRepository;
            _accountManagementService = accountManagementService;
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(WithdrawCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.TryInsertOrModifyAsync(OperationName, command.OperationId, async old =>
            {
                if (old != null)
                {
                    return null;
                }

                var account =
                    await _accountManagementService.GetByClientAndIdAsync(command.ClientId, command.AccountId);
                if (account == null || account.IsDisabled)
                {
                    publisher.PublishEvent(new WithdrawalFailedEvent(command.ClientId, command.AccountId,
                        command.Amount, command.OperationId, command.Reason));
                    return States.Failed.ToString();
                }
                else
                {
                    publisher.PublishEvent(_convertService.Convert<WithdrawalStartedInternalEvent>(command));
                    return States.Received.ToString();
                }
            });
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task<CommandHandlingResult> Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            return Fail(command, publisher);
        }

        private async Task<CommandHandlingResult> Fail(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.SetStateAsync(OperationName, command.OperationId,
                States.Failed.ToString());
            publisher.PublishEvent(new WithdrawalFailedEvent(command.ClientId, command.AccountId,
                command.Amount, command.OperationId, command.Reason));
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task<CommandHandlingResult> Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            return Complete(command, publisher);
        }

        private async Task<CommandHandlingResult> Complete(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.SetStateAsync(OperationName, command.OperationId,
                States.Finished.ToString());
            publisher.PublishEvent(new WithdrawalSucceededEvent(command.ClientId, command.AccountId,
                command.Amount, command.OperationId, command.Reason));
            return CommandHandlingResult.Ok();
        }

        private enum States
        {
            Received = 1,
            Finished = 2,
            Failed = 3,
        }
    }
}