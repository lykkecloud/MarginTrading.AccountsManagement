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

        public WithdrawalSucceededEvent([NotNull] string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
        }
    }
}