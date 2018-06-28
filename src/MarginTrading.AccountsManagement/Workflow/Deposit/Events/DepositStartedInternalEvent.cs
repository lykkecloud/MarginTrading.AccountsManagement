using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class DepositStartedInternalEvent : BaseEvent
    {
        [Key(2)]
        public string ClientId { get; }

        [Key(3)]
        public string AccountId { get; }

        [Key(4)]
        public decimal Amount { get; }

        [Key(5)]
        public string Comment { get; }

        [Key(6)]
        public string AuditLog { get; }

        public DepositStartedInternalEvent(string operationId, DateTime _, string clientId, string accountId, 
            decimal amount, [NotNull] string comment, string auditLog)
            : base(operationId)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "");

            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            Comment = comment;
            AuditLog = auditLog;
        }
    }
}