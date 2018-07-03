using System;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.Deposit.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    internal class DepositCommandsHandler
    {
        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;

        public DepositCommandsHandler(IConvertService convertService, ISystemClock systemClock)
        {
            _convertService = convertService;
            _systemClock = systemClock;
        }

        /// <summary>
        /// Handles the command to begin deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(DepositCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositStartedInternalEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime, 
                c.ClientId, c.AccountId, c.Amount, c.Comment, c.AuditLog));
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
            publisher.PublishEvent(new DepositSucceededEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime, c.ClientId, 
                c.AccountId, c.Amount));
        }
    }
}