using System;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    public class BeginDepositCommand : AccountBalanceOperationCommandBase
    {
        public BeginDepositCommand(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "");
        }
    }
}