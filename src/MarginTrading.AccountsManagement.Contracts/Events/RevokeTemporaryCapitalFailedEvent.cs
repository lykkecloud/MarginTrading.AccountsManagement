using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    [MessagePackObject]
    public class RevokeTemporaryCapitalFailedEvent : BaseEvent
    {
        public RevokeTemporaryCapitalFailedEvent([NotNull] string operationId, DateTime eventTimestamp, 
            string failReason, string eventSourceId, string revokeEventSourceId)
            : base(operationId, eventTimestamp)
        {
            FailReason = failReason;
            EventSourceId = eventSourceId;
            RevokeEventSourceId = revokeEventSourceId;
        }
     
        [Key(2)] 
        public string FailReason { get; }

        [Key(3)]
        public string EventSourceId { get; set; }
        
        [Key(4)]
        public string RevokeEventSourceId { get; }
    }
}