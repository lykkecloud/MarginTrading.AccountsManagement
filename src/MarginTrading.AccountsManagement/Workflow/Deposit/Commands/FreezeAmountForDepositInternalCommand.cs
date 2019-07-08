// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Commands
{
    [MessagePackObject]
    internal class FreezeAmountForDepositInternalCommand
    {
        [Key(0)]
        public string OperationId { get; }

        public FreezeAmountForDepositInternalCommand(string operationId)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        }
    }
}