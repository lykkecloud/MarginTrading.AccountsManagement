// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Margin unfreezing has succeeded
    /// </summary>
    [MessagePackObject]
    public class UnfreezeMarginSucceededWithdrawalEvent : BaseEvent
    {
        [Key(2)]
        public string ClientId { get; }

        [Key(3)]
        public string AccountId { get; }

        [Key(4)]
        public decimal Amount { get; }

        public UnfreezeMarginSucceededWithdrawalEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string clientId, [NotNull] string accountId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}