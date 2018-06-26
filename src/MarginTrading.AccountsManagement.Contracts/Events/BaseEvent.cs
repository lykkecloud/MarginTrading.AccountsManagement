using System;
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
        public string OperationId { get; protected set; }
        
        [Key(1)]
        public DateTime EventTimestamp { get; }

        protected BaseEvent()
        {
            EventTimestamp = DateTime.UtcNow;
        }
    }
}