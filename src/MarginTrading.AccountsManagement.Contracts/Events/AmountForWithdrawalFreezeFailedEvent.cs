using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFreezeFailedEvent : AccountBalanceBaseMessage
    {
        public AmountForWithdrawalFreezeFailedEvent(string operationId, DateTime eventTimestamp, 
            string accountId, decimal amount, string reason)
            : base(operationId, eventTimestamp, accountId, amount, reason)
        {
        }
    }
}