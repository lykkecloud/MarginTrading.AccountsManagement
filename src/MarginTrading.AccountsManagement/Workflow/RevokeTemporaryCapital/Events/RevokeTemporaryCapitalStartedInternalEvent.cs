// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Events
{
    [MessagePackObject]
    public class RevokeTemporaryCapitalStartedInternalEvent : BaseEvent
    {
        public RevokeTemporaryCapitalStartedInternalEvent(string operationId, DateTime eventTimestamp,
            List<InternalModels.TemporaryCapital> revokedTemporaryCapital)
            : base(operationId, eventTimestamp)
        {
            RevokedTemporaryCapital = revokedTemporaryCapital;
        }

        [Key(2)] 
        public List<InternalModels.TemporaryCapital> RevokedTemporaryCapital { get; }
    }
}