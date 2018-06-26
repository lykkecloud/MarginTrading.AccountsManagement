using System;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Events;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    public class FreezeAmountForWithdrawalCommand : AccountBalanceBaseEvent
    {
        public FreezeAmountForWithdrawalCommand(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}