using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IOperationExecutionInfoRepository
    {
        Task<OperationExecutionInfo<TData>> GetOrAddAsync<TData>(string operationName, string operationId,
            Func<OperationExecutionInfo<TData>> factory) where TData : class;

        [ItemNotNull]
        Task<OperationExecutionInfo<TData>> GetAsync<TData>(string operationName, string id) where TData : class;
        Task Save<TData>(OperationExecutionInfo<TData> executionInfo) where TData : class;
    }
}