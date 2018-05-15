using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;

namespace MarginTrading.AccountsManagement.TradingEngineMock
{
    /// <summary>
    /// This is a mock of whats going to exist in the backend
    /// </summary>
    public class FreezeAmountForWithdrawalCommandsHandler
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IConvertService _convertService;

        public FreezeAmountForWithdrawalCommandsHandler(IAccountManagementService accountManagementService,
            IConvertService convertService)
        {
            _accountManagementService = accountManagementService;
            _convertService = convertService;
        }

        /// <summary>
        /// Freeze the the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(FreezeAmountForWithdrawalCommand command, IEventPublisher publisher)
        {
            var account = await _accountManagementService.GetByClientAndIdAsync(command.ClientId, command.AccountId);
            if (account != null && account.Balance > command.Amount)
            {
                publisher.PublishEvent(_convertService.Convert<AmountForWithdrawalFrozenEvent>(command));
            }
            else
            {
                publisher.PublishEvent(new AmountForWithdrawalFreezeFailedEvent(command.ClientId, command.AccountId,
                    command.Amount, command.OperationId, account != null ? "Not enough free margin" : "Account not found"));
            }
            
            return CommandHandlingResult.Ok();
        }
    }
}