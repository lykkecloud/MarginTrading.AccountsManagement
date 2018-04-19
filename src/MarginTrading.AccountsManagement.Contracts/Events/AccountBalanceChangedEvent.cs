using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AccountBalanceChangedEvent
    {
        public string ClientId { get; }
        public string AccountId { get; }
        public decimal AmountDelta { get; }
        public string OperationId { get; }
        public string Reason { get; }

        public AccountBalanceChangedEvent(string userId, string accountId, decimal amountDelta, string operationId,
            string reason)
        {
            ClientId = userId ?? throw new ArgumentNullException(nameof(userId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AmountDelta = amountDelta;
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}