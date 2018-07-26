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