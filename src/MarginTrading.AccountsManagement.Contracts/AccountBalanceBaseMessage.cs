// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts
{
    [MessagePackObject]
    public abstract class AccountBalanceBaseMessage
    {
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public DateTime EventTimestamp { get; }

        [Key(2)]
        public string AccountId { get; }

        [Key(3)]
        public decimal Amount { get; }

        [Key(4)]
        public string Reason { get; }

        [SerializationConstructor]
        public AccountBalanceBaseMessage([NotNull] string operationId, DateTime _, 
            [NotNull] string accountId, decimal amount, [NotNull] string reason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            this.AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            this.Amount = amount;
            this.Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            
            EventTimestamp = DateTime.UtcNow;
        }
    }
}