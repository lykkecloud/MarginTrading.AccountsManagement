using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFrozenEvent : AccountBalanceBaseMessage
    {
        public AmountForWithdrawalFrozenEvent(string operationId, DateTime _, string clientId, string accountId, 
            decimal amount, string reason)
            : base(operationId, _, clientId, accountId, amount, reason)
        {
        }
    }
}