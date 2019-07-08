// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Events
{
    [MessagePackObject]
    public class DeleteAccountsStartedInternalEvent: BaseEvent
    {
        [Key(2)]
        public Dictionary<string, string> FailedAccountIds { get; }
        
        public DeleteAccountsStartedInternalEvent(string operationId, DateTime eventTimestamp, 
            Dictionary<string, string> failedAccountIds)
            : base(operationId, eventTimestamp)
        {
            FailedAccountIds = failedAccountIds;
        }
    }
}