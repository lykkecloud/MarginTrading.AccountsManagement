using System;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Events;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    public class FreezeAmountForWithdrawalCommand : AccountBalanceBaseMessage
    {
        public FreezeAmountForWithdrawalCommand(string operationId, DateTime eventTimestamp, string accountId, 
            decimal amount, string reason)
            : base(operationId, eventTimestamp, accountId, amount, reason)
        {
        }
    }
}