using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.InternalModels
{
    internal class OperationExecutionInfo<T> : IOperationExecutionInfo<T> 
        where T: class
    {
        public string OperationName { get; }
        public string Id { get; }

        public T Data { get; }

        public OperationExecutionInfo([NotNull] string operationName, [NotNull] string id, 
            [NotNull] T data)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}