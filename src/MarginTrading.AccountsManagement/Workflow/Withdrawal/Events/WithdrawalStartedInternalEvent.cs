using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    /// <summary>
    /// Withdrawal started
    /// </summary>
    [MessagePackObject]
    public class WithdrawalStartedInternalEvent : BaseEvent
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

        public WithdrawalStartedInternalEvent(string operationId, DateTime eventTimestamp, string clientId, string accountId,
            decimal amount, [NotNull] string comment, string auditLog)
            : base(operationId, eventTimestamp)
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