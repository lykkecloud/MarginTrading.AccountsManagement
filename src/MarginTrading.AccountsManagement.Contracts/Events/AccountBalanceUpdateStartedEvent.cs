using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AccountBalanceUpdateStartedEvent : AccountBalanceOperationEventBase
    {
        [Key(5)] public string Source { get; }

        public AccountBalanceUpdateStartedEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason, string source) : base(clientId, accountId, amount, operationId, reason)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }
    }
}