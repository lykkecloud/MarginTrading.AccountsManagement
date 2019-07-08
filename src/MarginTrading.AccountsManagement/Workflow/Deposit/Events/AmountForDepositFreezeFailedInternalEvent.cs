// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class AmountForDepositFreezeFailedInternalEvent: BaseEvent
    {
        public AmountForDepositFreezeFailedInternalEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string reason)
            : base(operationId, eventTimestamp)
        {
        }
    }
}