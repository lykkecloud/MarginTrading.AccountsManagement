// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        public DateTime LastModified { get; }

        public T Data { get; }

        public OperationExecutionInfo([NotNull] string operationName, [NotNull] string id, 
            [NotNull] T data, DateTime lastModified)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            Id = id ?? throw new ArgumentNullException(nameof(id));
            LastModified = lastModified;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }
    }
}