using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class WithdrawalStartedEvent : AccountOperationEventBase
    {
        public WithdrawalStartedEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}