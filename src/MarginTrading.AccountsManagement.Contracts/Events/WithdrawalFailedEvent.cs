using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Withdrawal operation failed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalFailedEvent : BaseEvent
    {
        [Key(2)] public string Reason { get; }

        public WithdrawalFailedEvent([NotNull] string operationId, DateTime eventTimestamp, string reason)
            : base(operationId, eventTimestamp)
        {
            Reason = reason;
        }
    }
}