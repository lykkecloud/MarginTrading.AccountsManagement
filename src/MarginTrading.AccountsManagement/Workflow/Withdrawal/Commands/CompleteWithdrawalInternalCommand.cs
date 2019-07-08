// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands
{
    [MessagePackObject]
    internal class CompleteWithdrawalInternalCommand
    {
        [Key(0)]
        public string OperationId { get; }

        public CompleteWithdrawalInternalCommand([NotNull] string operationId)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        }
    }
}