using System;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.Deposit.Events;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    internal class DepositCommandsHandler
    {
        private readonly IConvertService _convertService;

        public DepositCommandsHandler(IConvertService convertService)
        {
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to begin deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(DepositCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositStartedInternalEvent(c.OperationId, new DateTime(), c.ClientId,
                c.AccountId, c.Amount, c.Comment, c.AuditLog));
        }

        /// <summary>
        /// Handles the command to freeze amount for deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(FreezeAmountForDepositInternalCommand c, IEventPublisher publisher)
        {
            // todo: Now it always succeeds. Will be used for deposit limiting.
            publisher.PublishEvent(new AmountForDepositFrozenInternalEvent(c.OperationId));
        }

        /// <summary>
        /// Handles the command to fail deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(FailDepositInternalCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositFailedEvent(c.OperationId));
        }

        /// <summary>
        /// Handles the command to complete deposit
        /// </summary>
        [UsedImplicitly]
        private void Handle(CompleteDepositInternalCommand c, IEventPublisher publisher)
        {
            publisher.PublishEvent(new DepositSucceededEvent(c.OperationId, new DateTime(), c.ClientId, c.AccountId,
                c.Amount));
        }
    }
}