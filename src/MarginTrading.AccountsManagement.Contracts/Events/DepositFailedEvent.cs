using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Deposit operation failed
    /// </summary>
    [MessagePackObject]
    public class DepositFailedEvent : BaseEvent
    {
        public DepositFailedEvent([NotNull] string operationId, DateTime eventTimestamp) 
            : base(operationId, eventTimestamp)
        {
        }
    }
}