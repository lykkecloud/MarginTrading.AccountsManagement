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

        public WithdrawalFailedEvent([NotNull] string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
        }
    }
}