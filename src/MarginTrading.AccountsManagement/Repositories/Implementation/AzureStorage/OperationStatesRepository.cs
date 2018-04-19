using System;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class OperationStatesRepository : IOperationStatesRepository
    {
        private readonly IConvertService _convertService;
        private readonly INoSQLTableStorage<OperationStateEntity> _tableStorage;

        public OperationStatesRepository(IConvertService convertService, INoSQLTableStorage<OperationStateEntity> tableStorage)
        {
            _convertService = convertService;
            _tableStorage = tableStorage;
        }

        public Task<bool> InsertIfNotExistsAsync(OperationState operationState)
        {
            return _tableStorage.TryInsertAsync(_convertService.Convert<OperationStateEntity>(operationState));
        }

        public async Task<OperationState> Get(string operationName, string id)
        {
            var entity = await _tableStorage.GetDataAsync(OperationStateEntity.GeneratePartitionKey(operationName),
                OperationStateEntity.GenerateRowKey(id));
            return entity == null ? null : _convertService.Convert<OperationState>(entity);
        }

        public async Task<bool> TryChangeState<TState>(string operationName, string id, TState oldState, Func<TState> newStateFunc)
        {
            var success = false;
            await _tableStorage.MergeAsync(OperationStateEntity.GeneratePartitionKey(operationName),
                OperationStateEntity.GenerateRowKey(id), a =>
                {
                    success = false;
                    if (a.State == oldState.ToString())
                    {
                        a.State = newStateFunc().ToString();
                        success = true;
                    }
                    
                    return a;
                });
            
            return success;
        }

        public Task ChangeState<TState>(string operationName, string id, TState newState)
        {
            return _tableStorage.MergeAsync(OperationStateEntity.GeneratePartitionKey(operationName),
                OperationStateEntity.GenerateRowKey(id), a =>
                {
                    a.State = newState.ToString();
                    return a;
                });
        }
    }
}