using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IOperationExecutionInfoRepository
    {
        Task<IOperationExecutionInfo<TData>> GetOrAddAsync<TData>(string operationName, string operationId,
            Func<IOperationExecutionInfo<TData>> factory) where TData : class;

        [ItemCanBeNull]
        Task<IOperationExecutionInfo<TData>> GetAsync<TData>(string operationName, string id) where TData : class;
        
        Task SaveAsync<TData>(IOperationExecutionInfo<TData> executionInfo) where TData : class;
        
        Task DeleteAsync<TData>(IOperationExecutionInfo<TData> executionInfo) where TData : class;
    }
}