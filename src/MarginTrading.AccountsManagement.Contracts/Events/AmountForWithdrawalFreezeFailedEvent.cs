using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFreezeFailedEvent : AccountBalanceBaseMessage
    {
        public AmountForWithdrawalFreezeFailedEvent(string operationId, DateTime eventTimestamp, string clientId, 
            string accountId, decimal amount, string reason)
            : base(operationId, eventTimestamp, clientId, accountId, amount, reason)
        {
        }
    }
}