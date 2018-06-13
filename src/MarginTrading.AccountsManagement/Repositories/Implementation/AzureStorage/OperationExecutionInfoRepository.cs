using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class OperationExecutionInfoRepository : IOperationExecutionInfoRepository
    {
        private readonly INoSQLTableStorage<OperationExecutionInfoEntity> _tableStorage;
        private readonly ILog _log;
        private readonly bool _enableOperationsLogs;

        public OperationExecutionInfoRepository(IReloadingManager<AccountManagementSettings> settings,
            IAzureTableStorageFactoryService azureTableStorageFactoryService, ILog log)
        {
            _enableOperationsLogs = settings.CurrentValue.EnableOperationsLogs;
            _tableStorage = azureTableStorageFactoryService.Create<OperationExecutionInfoEntity>(
                settings.Nested(s => s.Db.ConnectionString),
                "OperationExecutionInfo",
                log);
            _log = log.CreateComponentScope(nameof(OperationExecutionInfoRepository));
        }
        
        public async Task<IOperationExecutionInfo<TData>> GetOrAddAsync<TData>(string operationName, string operationId,
            Func<IOperationExecutionInfo<TData>> factory) where TData : class
        {
            var entity = await _tableStorage.GetOrInsertAsync(
                partitionKey: OperationExecutionInfoEntity.GeneratePartitionKey(operationName),
                rowKey: OperationExecutionInfoEntity.GeneratePartitionKey(operationId),
                createNew: () => Convert(factory()));
                
            return Convert<TData>(entity);
        }

        public async Task<IOperationExecutionInfo<TData>> GetAsync<TData>(string operationName, string id)
            where TData : class
        {
            var obj = await _tableStorage.GetDataAsync(
                          OperationExecutionInfoEntity.GeneratePartitionKey(operationName),
                          OperationExecutionInfoEntity.GenerateRowKey(id)) ?? throw new InvalidOperationException(
                          $"Operation execution info for {operationName} #{id} not yet exists");
            
            return Convert<TData>(obj);
        }

        public async Task Save<TData>(IOperationExecutionInfo<TData> executionInfo) where TData : class
        {
            await _tableStorage.ReplaceAsync(Convert(executionInfo));
        }

        private static IOperationExecutionInfo<TData> Convert<TData>(OperationExecutionInfoEntity entity)
            where TData : class
        {
            return new OperationExecutionInfo<TData>(
                operationName: entity.OperationName,
                id: entity.Id,
                data: entity.Data is string dataStr
                    ? JsonConvert.DeserializeObject<TData>(dataStr)
                    : ((JToken) entity.Data).ToObject<TData>());
        }

        private static OperationExecutionInfoEntity Convert<TData>(IOperationExecutionInfo<TData> model)
            where TData : class
        {
            return new OperationExecutionInfoEntity
            {
                Id = model.Id,
                OperationName = model.OperationName,
                Data = model.Data.ToJson(),
            };
        }
    }
}