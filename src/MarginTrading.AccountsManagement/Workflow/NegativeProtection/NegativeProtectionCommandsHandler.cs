using System;
using System.Threading.Tasks;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.NegativeProtection.Commands;

namespace MarginTrading.AccountsManagement.Workflow.NegativeProtection
{
    internal class NegativeProtectionCommandsHandler
    {
        private readonly AccountManagementSettings _accountManagementSettings;

        public NegativeProtectionCommandsHandler(
            AccountManagementSettings accountManagementSettings)
        {
            _accountManagementSettings = accountManagementSettings;
        }

        public void Handle(NotifyNegativeProtectionInternalCommand command, IEventPublisher publisher)
        {
            //skipping idempotency violation check
            
            publisher.PublishEvent(new NegativeProtectionEvent(
                id: Guid.NewGuid().ToString("N"),
                correlationId: command.CorrelationId,
                causationId: command.CausationId,
                eventTimestamp: command.EventTimestamp,
                clientId: command.ClientId,
                accountId: command.AccountId,
                amount: command.Amount,
                isAutoCompensated: _accountManagementSettings.NegativeProtectionAutoCompensation
            ));
        }
    }
}