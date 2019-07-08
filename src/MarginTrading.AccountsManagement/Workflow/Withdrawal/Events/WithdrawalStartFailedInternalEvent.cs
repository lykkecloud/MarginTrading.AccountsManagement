// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    [MessagePackObject]
    public class WithdrawalStartFailedInternalEvent : BaseEvent
    {
        [Key(2)]
        public string Reason { get; }
        
        public WithdrawalStartFailedInternalEvent([NotNull] string operationId, DateTime eventTimestamp, string reason)
            : base(operationId, eventTimestamp)
        {
            Reason = reason;
        }
    }
}