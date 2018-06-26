using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Base event containing operation Id and timestamp
    /// </summary>
    [MessagePackObject]
    public abstract class BaseEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public DateTime EventTimestamp { get; }

        protected BaseEvent([NotNull] string operationId)
        {
            OperationId = operationId;
            EventTimestamp = DateTime.UtcNow;
        }
    }
}