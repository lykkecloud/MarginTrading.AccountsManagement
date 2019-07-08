// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    [MessagePackObject]
    public class UnfreezeMarginOnFailSucceededWithdrawalEvent : BaseEvent
    {
        [Key(2)]
        public string AccountId { get; }

        [Key(3)]
        public decimal Amount { get; }

        public UnfreezeMarginOnFailSucceededWithdrawalEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string accountId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}