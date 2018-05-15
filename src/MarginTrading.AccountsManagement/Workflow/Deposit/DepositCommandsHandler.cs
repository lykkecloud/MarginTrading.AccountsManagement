using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    internal class DepositCommandsHandler
    {
        private const string OperationName = "Deposit";
        private readonly IOperationStatesRepository _operationStatesRepository;
        private readonly IConvertService _convertService;

        public DepositCommandsHandler(IOperationStatesRepository operationStatesRepository,
            IConvertService convertService)
        {
            _operationStatesRepository = operationStatesRepository;
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to begin deposit
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(BeginDepositCommand command, IEventPublisher publisher)
        {
            if (await _operationStatesRepository.TryInsertAsync(OperationName, command.OperationId,
                States.Received.ToString()))
            {
                publisher.PublishEvent(_convertService.Convert<DepositStartedEvent>(command));
            }
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to freeze amount for deposit
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(FreezeAmountForDepositInternalCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.TryInsertOrModifyAsync(OperationName, command.OperationId,
                oldState =>
                {
                    if (oldState != States.Received.ToString())
                    {
                        return null;
                    }

                    // todo: Now it always succeeds. Will be used for deposit limiting.
                    publisher.PublishEvent(_convertService.Convert<AmountForDepositFrozenEvent>(command));
                    return Task.FromResult(States.Frozen.ToString());
                });
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to fail deposit
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(FailDepositInternalCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.SetStateAsync(OperationName, command.OperationId,
                States.Failed.ToString());
            publisher.PublishEvent(_convertService.Convert<DepositFailedEvent>(command));
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles the command to complete deposit
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(CompleteDepositInternalCommand command, IEventPublisher publisher)
        {
            await _operationStatesRepository.SetStateAsync(OperationName, command.OperationId,
                States.Finished.ToString());
            publisher.PublishEvent(_convertService.Convert<DepositCompletedEvent>(command));
            return CommandHandlingResult.Ok();
        }

        private enum States
        {
            Received = 1,
            Frozen = 2,
            Finished = 3,
            Failed = 4,
        }
    }
}