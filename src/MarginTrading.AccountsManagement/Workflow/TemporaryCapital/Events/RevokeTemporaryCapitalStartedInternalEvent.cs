using System;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Events
{
    [MessagePackObject]
    public class RevokeTemporaryCapitalStartedInternalEvent : BaseEvent
    {
        public RevokeTemporaryCapitalStartedInternalEvent(string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
           
        }
    }
}