using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.Commands;

namespace MarginTrading.AccountsManagement.Workflow.CommandHandlers
{
    public class UpdateBalanceCommandHandler
    {
        private readonly IAccountManagementService _accountManagementService;

        public UpdateBalanceCommandHandler(IAccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }

        /// <summary>
        /// Handles the command to change the balance
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(UpdateBalanceInternalCommand command, IEventPublisher publisher)
        {
            await _accountManagementService.ChargeManuallyAsync(command.ClientId, command.AccountId, command.AmountDelta,
                command.Reason);
            
            publisher.PublishEvent(new AccountBalanceChangedEvent(command.ClientId, command.AccountId,
                command.AmountDelta, command.OperationId, command.Reason));
        }
    }
}