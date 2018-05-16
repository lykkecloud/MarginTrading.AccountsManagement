using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    internal class UpdateBalanceCommandsHandler
    {
        private const string OperationName = "UpdateBalance";
        private readonly IAccountManagementService _accountManagementService;
        private readonly IOperationStatesRepository _operationStatesRepository;
        private readonly IConvertService _convertService;

        public UpdateBalanceCommandsHandler(IAccountManagementService accountManagementService,
            IOperationStatesRepository operationStatesRepository, IConvertService convertService)
        {
            _accountManagementService = accountManagementService;
            _operationStatesRepository = operationStatesRepository;
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to begin changing the balance
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(BeginBalanceUpdateInternalCommand command,
            IEventPublisher publisher)
        {
            if (await _operationStatesRepository.TryInsertAsync(OperationName, command.OperationId,
                States.Received.ToString()))
            {
                publisher.PublishEvent(_convertService.Convert<AccountBalanceUpdateStartedEvent>(command));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to begin changing the balance because of a closed position
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(BeginClosePositionBalanceUpdateCommand command,
            IEventPublisher publisher)
        {
            if (await _operationStatesRepository.TryInsertAsync(OperationName, command.OperationId,
                States.Received.ToString()))
            {
                publisher.PublishEvent(new AccountBalanceUpdateStartedEvent(command.ClientId, command.AccountId,
                    command.Amount, command.OperationId, command.Reason, "ClosePosition")); // todo add command.EventSourceId
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to change the balance
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(UpdateBalanceInternalCommand command,
            IEventPublisher publisher)
        {
            await _operationStatesRepository.TryInsertOrModifyAsync(OperationName, command.OperationId,
                async oldState =>
                {
                    if (oldState != States.Received.ToString())
                    {
                        return null;
                    }

                    await _accountManagementService.ChargeManuallyAsync(command.ClientId, command.AccountId,
                        command.Amount, command.Reason);
                    publisher.PublishEvent(_convertService.Convert<AccountBalanceChangedEvent>(command));
                    return States.Finished.ToString();
                });

            return CommandHandlingResult.Ok();
        }

        private enum States
        {
            Received = 1,
            Finished = 2,
        }
    }
}