// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    [MessagePackObject]
    public class UnfreezeMarginOnFailWithdrawalCommand
    {
        [Key(0)][NotNull]
        public string OperationId { get; }

        public UnfreezeMarginOnFailWithdrawalCommand([NotNull] string operationId)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        }
    }
}