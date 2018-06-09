using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.InternalModels
{
    internal class OperationExecutionInfo<T> : IOperationExecutionInfo<T> 
        where T: class
    {
        [CanBeNull]
        public string Version { get; }
        public string OperationName { get; }
        public string Id { get; }

        public T Data { get; }

        public OperationExecutionInfo([CanBeNull] string version, [NotNull] string operationName, [NotNull] string id, 
            [NotNull] T data)
        {
            Version = version;
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}