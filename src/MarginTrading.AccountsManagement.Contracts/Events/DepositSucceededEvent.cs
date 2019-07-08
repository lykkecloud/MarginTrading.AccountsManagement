// Copyright (c) 2019 Lykke Corp.

 using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// The deposit operation has succeeded
    /// </summary>
    [MessagePackObject]
    public class DepositSucceededEvent : BaseEvent
    {
        [Key(2)] public string ClientId { get; }

        [Key(3)] public string AccountId { get; }

        [Key(4)] public decimal Amount { get; }

        public DepositSucceededEvent([NotNull] string operationId, DateTime eventTimestamp, [CanBeNull] string clientId,
            [NotNull] string accountId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            ClientId = clientId;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;

        }
    }
}