using System;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Starts money withdrawal for <see cref="AccountOperationCommandBase.ClientId"/>
    /// from <see cref="AccountOperationCommandBase.AccountId"/>
    /// </summary>
    public class BeginWithdrawalCommand : AccountOperationCommandBase
    {
        public BeginWithdrawalCommand(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}