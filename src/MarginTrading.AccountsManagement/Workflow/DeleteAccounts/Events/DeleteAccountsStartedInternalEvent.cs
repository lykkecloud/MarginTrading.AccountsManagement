using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Events
{
    [MessagePackObject]
    public class DeleteAccountsStartedInternalEvent: BaseEvent
    {
        public DeleteAccountsStartedInternalEvent(string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
           
        }
    }
}