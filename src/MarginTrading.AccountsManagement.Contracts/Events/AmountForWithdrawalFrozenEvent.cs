using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFrozenEvent : AccountBalanceBaseMessage
    {
        public AmountForWithdrawalFrozenEvent(string operationId, DateTime eventTimestamp, string clientId, 
            string accountId, decimal amount, string reason)
            : base(operationId, eventTimestamp, clientId, accountId, amount, reason)
        {
        }
    }
}