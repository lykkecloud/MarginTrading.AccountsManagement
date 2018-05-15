using MarginTrading.AccountsManagement.Contracts.Commands;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands
{
    internal class UpdateBalanceInternalCommand : AccountBalanceOperationCommandBase
    {
        [Key(5)] public string Source { get; }

        public UpdateBalanceInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason, string source) : base(clientId, accountId, amount, operationId, reason)
        {
            Source = source;
        }
    }
}