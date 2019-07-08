// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class AmountForDepositFrozenInternalEvent: BaseEvent
    {
        public AmountForDepositFrozenInternalEvent([NotNull] string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
        }
    }
}