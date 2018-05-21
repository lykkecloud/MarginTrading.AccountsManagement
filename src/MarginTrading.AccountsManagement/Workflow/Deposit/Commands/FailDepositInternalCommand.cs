using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Commands
{
    public class FailDepositInternalCommand : AccountBalanceTransactionMessageBase
    {
        public FailDepositInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}