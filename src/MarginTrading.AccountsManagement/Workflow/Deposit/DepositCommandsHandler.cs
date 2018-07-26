using System;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.Deposit.Events;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    internal class DepositCommandsHandler
    {
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private const string OperationName = "Withdraw";
        private readonly IChaosKitty _chaosKitty;

        public DepositCommandsHandler(ISystemClock systemClock, IOperationExecutionInfoRepository executionInfoRepository, IChaosKitty chaosKitty)
        {
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Handles the command to begin deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(DepositCommand c, IEventPublisher publisher)
        {
            var executionInfo = _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: c.OperationId,
                factory: () => new OperationExecutionInfo<WithdrawalDepositData>(
                    operationName: OperationName,
                    id: c.OperationId,
                    data: new WithdrawalDepositData 
                    {
                        ClientId = c.ClientId,
                        AccountId = c.AccountId,
                        Amount = c.Amount,
                        AuditLog = c.AuditLog,
                        State = State.Created,
                        Comment = c.Comment
                    }));

            _chaosKitty.Meow(c.OperationId);

            publisher.PublishEvent(new DepositStartedInternalEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime));
            
        }

        /// <summary>
        /// Handles the command to freeze amount for deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(FreezeAmountForDepositInternalCommand c, IEventPublisher publisher)
        {
            // todo: Now it always succeeds. Will be used for deposit limiting.
            publisher.PublishEvent(new AmountForDepositFrozenInternalEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime));
        }

        /// <summary>
        /// Handles the command to fail deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(FailDepositInternalCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositFailedEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime));
        }

        /// <summary>
        /// Handles the command to complete deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(CompleteDepositInternalCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositSucceededEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime));
        }
    }
}