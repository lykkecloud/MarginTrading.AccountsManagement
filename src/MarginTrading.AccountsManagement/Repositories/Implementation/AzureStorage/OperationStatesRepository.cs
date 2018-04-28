using System;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using JetBrains.Annotations;
using Lykke.AzureStorage;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class OperationStatesRepository : IOperationStatesRepository
    {
        private readonly INoSQLTableStorage<OperationStateEntity> _tableStorage;
        private readonly ILog _log;
        private readonly bool _enableOperationsLogs;

        public OperationStatesRepository(IReloadingManager<AccountManagementSettings> settings,
            IAzureTableStorageFactoryService azureTableStorageFactoryService, ILog log)
        {
            _enableOperationsLogs = settings.CurrentValue.EnableOperationsLogs;
            _tableStorage = azureTableStorageFactoryService.Create<OperationStateEntity>(
                settings.Nested(s => s.Db.ConnectionString), "OperationStates", log);
            _log = log.CreateComponentScope(nameof(OperationStatesRepository));
        }

        public async Task<bool> TryInsertOrModifyAsync(string operationName, string id,
            Func<string, Task<string>> modify)
        {
            while (true)
            {
                try
                {
                    var existingEntity = await GetDataAsync(operationName, id);
                    var oldState = existingEntity?.State;
                    var newState = await modify(oldState);
                    if (newState == null)
                    {
                        Log(nameof(TryInsertOrModifyAsync), existingEntity,
                            "Did not insert or replace state from " + oldState);
                        return false;
                    }

                    if (existingEntity != null)
                    {
                        existingEntity.State = newState;
                        await _tableStorage.ReplaceAsync(existingEntity);
                        Log(nameof(TryInsertOrModifyAsync), existingEntity,
                            "Replaced state from " + oldState);
                    }
                    else
                    {
                        var newEntity =
                            new OperationStateEntity {Id = id, OperationName = operationName, State = newState};
                        await _tableStorage.InsertAsync(newEntity);
                        Log(nameof(TryInsertOrModifyAsync), newEntity, "Inserted");
                    }

                    return true;
                }
                catch (OptimisticConcurrencyException)
                {
                }
                catch (StorageException e) when (
                    e.RequestInformation.ExtendedErrorInformation.ErrorCode ==
                    TableErrorCodeStrings.UpdateConditionNotSatisfied ||
                    e.RequestInformation.ExtendedErrorInformation.ErrorCode ==
                    TableErrorCodeStrings.EntityAlreadyExists ||
                    e.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.EntityNotFound)
                {
                }
            }
        }

        public async Task<bool> TryInsertAsync(string operationName, string id, string state)
        {
            var operationStateEntity = new OperationStateEntity
            {
                Id = id,
                OperationName = operationName,
                State = state
            };
            var result = await _tableStorage.TryInsertAsync(operationStateEntity);
            Log(nameof(TryInsertAsync), operationStateEntity,
                result ? "Inserted" : "Tried to insert, but already existed");
            return result;
        }

        public async Task SetStateAsync(string operationName, string id, string state)
        {
            var operationStateEntity = new OperationStateEntity
            {
                Id = id,
                OperationName = operationName,
                State = state,
            };
            await _tableStorage.InsertOrMergeAsync(operationStateEntity);
            Log(nameof(SetStateAsync), operationStateEntity, "Inserted or merged");
        }

        [ItemCanBeNull]
        private Task<OperationStateEntity> GetDataAsync(string operationName, string id)
        {
            return _tableStorage.GetDataAsync(OperationStateEntity.GeneratePartitionKey(operationName),
                OperationStateEntity.GenerateRowKey(id));
        }

        private void Log(string process, OperationStateEntity context, string info)
        {
            if (_enableOperationsLogs)
            {
                _log.WriteInfo(process, context,
                    info + " (op descr: " + context.OperationName + " #" + context.Id + " => " + context.State + ")");
            }
        }
    }
}