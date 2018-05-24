using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Starts money withdrawal
    /// </summary>
    public class WithdrawCommand
    {
        [Key(0)]
        public string OperationId { get; }

        [Key(1)]
        public string ClientId { get; }

        [Key(2)]
        public string AccountId { get; }

        [Key(3)]
        public decimal Amount { get; }

        [Key(4)]
        public string Comment { get; }

        [Key(5)]
        public string AuditLog { get; }

        public WithdrawCommand(string operationId, string clientId, string accountId, decimal amount, [NotNull] string comment, string auditLog)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "");

            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
            AuditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        }
    }
}