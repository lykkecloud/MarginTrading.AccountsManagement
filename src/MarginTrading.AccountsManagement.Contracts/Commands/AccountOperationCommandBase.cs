using System;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    public abstract class AccountOperationCommandBase
    {
        public string ClientId { get; }
        public string AccountId { get; }
        public decimal Amount { get; }
        public string OperationId { get; }
        public string Reason { get; }

        protected AccountOperationCommandBase(string clientId, string accountId, decimal amount, string operationId,
            string reason)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason;
        }
    }
}