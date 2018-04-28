using System;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands
{
    internal class BeginBalanceUpdateInternalCommand : AccountBalanceOperationCommandBase
    {
        [Key(5)] public string Source { get; }

        public BeginBalanceUpdateInternalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason, string source) : base(clientId, accountId, amount, operationId, reason)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }
    }
}