using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IOperationStatesRepository
    {
        [ItemCanBeNull]
        Task<OperationState> Get(string operationName, string id);

        Task<bool> InsertIfNotExistsAsync(OperationState operationState);
        Task<bool> TryChangeState<TState>(string operationName, string id, TState oldState, Func<TState> newStateFunc);
        Task ChangeState<TState>(string operationName, string id, TState newState);
    }
}