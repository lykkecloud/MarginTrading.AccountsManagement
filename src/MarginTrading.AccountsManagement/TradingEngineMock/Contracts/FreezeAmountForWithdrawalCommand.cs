using MarginTrading.AccountsManagement.Contracts.Commands;

namespace MarginTrading.AccountsManagement.TradingEngineMock.Contracts
{
    public class FreezeAmountForWithdrawalCommand : AccountBalanceOperationCommandBase
    {
        public FreezeAmountForWithdrawalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}