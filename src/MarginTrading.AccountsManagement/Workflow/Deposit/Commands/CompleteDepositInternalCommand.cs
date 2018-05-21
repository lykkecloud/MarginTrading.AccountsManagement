using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Commands
{
    public class CompleteDepositInternalCommand : AccountBalanceTransactionMessageBase
    {
        public CompleteDepositInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}