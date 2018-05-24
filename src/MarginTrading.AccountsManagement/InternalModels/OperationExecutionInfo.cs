using System;
using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.InternalModels
{
    internal class OperationExecutionInfo<T> where T: class
    {
        [CanBeNull]
        public string Version { get; }
        public string OperationName { get; }
        public string Id { get; }

        public T Data { get; }

        public OperationExecutionInfo([CanBeNull] string version, string operationName, string id, [NotNull] T data)
        {
            Version = version;
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}