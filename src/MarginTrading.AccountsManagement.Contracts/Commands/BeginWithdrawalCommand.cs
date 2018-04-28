using System;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Starts money withdrawal for <see cref="AccountBalanceOperationCommandBase.ClientId"/>
    /// from <see cref="AccountBalanceOperationCommandBase.AccountId"/>
    /// </summary>
    public class BeginWithdrawalCommand : AccountBalanceOperationCommandBase
    {
        public BeginWithdrawalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "");
        }
    }
}