using System;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Events
{
    [MessagePackObject]
    internal class GiveTemporaryCapitalStartedInternalEvent : BaseEvent
    {
        public GiveTemporaryCapitalStartedInternalEvent(string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
           
        }
    }
}