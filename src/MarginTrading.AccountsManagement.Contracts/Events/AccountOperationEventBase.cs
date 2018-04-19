using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public abstract class AccountOperationEventBase
    {
        public string ClientId { get; }
        public string AccountId { get; }
        public decimal Amount { get; }
        public string OperationId { get; }
        public string Reason { get; }

        protected AccountOperationEventBase(string clientId, string accountId, decimal amount, string operationId, string reason)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}