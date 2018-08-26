using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Happens when the withdrawal is completed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalSucceededEvent : BaseEvent
    {
        [Key(2)] public string ClientId { get; }

        [Key(3)] public string AccountId { get; }

        [Key(4)] public decimal Amount { get; }

        public WithdrawalSucceededEvent([NotNull] string operationId, DateTime eventTimestamp,
            [NotNull] string clientId, [NotNull] string accountId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}