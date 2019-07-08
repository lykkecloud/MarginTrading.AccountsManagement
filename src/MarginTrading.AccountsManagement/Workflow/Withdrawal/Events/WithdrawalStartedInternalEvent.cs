// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    /// <summary>
    /// Withdrawal started
    /// </summary>
    [MessagePackObject]
    public class WithdrawalStartedInternalEvent : BaseEvent
    {
        public WithdrawalStartedInternalEvent(string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
         
        }
    }
}