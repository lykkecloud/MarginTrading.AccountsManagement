using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands
{
    internal class CompleteWithdrawalInternalCommand
    {
        public CompleteWithdrawalInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}