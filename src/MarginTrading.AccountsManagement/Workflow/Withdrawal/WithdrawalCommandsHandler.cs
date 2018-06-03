using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalCommandsHandler
    {
        private readonly IConvertService _convertService;

        public WithdrawalCommandsHandler(IConvertService convertService)
        {
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(WithdrawCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(_convertService.Convert<WithdrawalStartedInternalEvent>(command));
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(_convertService.Convert<WithdrawalFailedEvent>(command));
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(_convertService.Convert<WithdrawalSucceededEvent>(command));
        }
    }
}