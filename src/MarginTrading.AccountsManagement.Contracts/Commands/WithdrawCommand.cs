// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Starts money withdrawal
    /// </summary>
    [MessagePackObject]
    public class WithdrawCommand
    {
        [Key(0)]
        public string OperationId { get; }

        /// <summary>
        /// Property is not used internally. It is here only to not to break the contract.
        /// </summary>
        [CanBeNull]
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

        public WithdrawCommand([NotNull] string operationId, [CanBeNull] string clientId, [NotNull] string accountId, 
            decimal amount, [NotNull] string comment, [CanBeNull] string auditLog)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "");

            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            ClientId = clientId;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            Comment = comment;
            AuditLog = auditLog;
        }
    }
}