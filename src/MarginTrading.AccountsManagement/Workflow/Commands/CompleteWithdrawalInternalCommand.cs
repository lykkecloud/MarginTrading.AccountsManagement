using MarginTrading.AccountsManagement.Contracts.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Commands
{
    public class CompleteWithdrawalInternalCommand : AccountOperationCommandBase
    {
        public CompleteWithdrawalInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}