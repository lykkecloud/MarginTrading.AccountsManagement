using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    /// <summary>
    /// Converts <see cref="AccountBalanceChangedEvent"/> to <see cref="AccountChangedEvent"/>
    /// </summary>
    public class AccountChangesProcess : IProcess
    {
        private ICommandSender _commandSender;
        private IEventPublisher _eventPublisher;

        public void Dispose()
        {
        }

        public void Start(ICommandSender commandSender, IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
            _commandSender = commandSender;
        }

        /// <summary>
        /// The balance has been changed - send account changed event
        /// </summary>
        [UsedImplicitly]
        private void Handle(AccountBalanceChangedEvent evt, ICommandSender sender)
        {
            _eventPublisher.PublishEvent(
                new AccountChangedEvent(
                    evt.Change.ChangeTimestamp,
                    evt.Account,
                    AccountChangedEventTypeContract.BalanceUpdated));
        }
    }
}