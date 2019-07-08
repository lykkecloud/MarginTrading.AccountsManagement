// Copyright (c) 2019 Lykke Corp.

using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.AccountsManagement.Repositories.AzureServices.Implementations
{
    public class AzureTableStorageFactoryService : IAzureTableStorageFactoryService
    {
        public INoSQLTableStorage<TEntity> Create<TEntity>(IReloadingManager<string> connectionStringManager,
            string tableName, ILog log) where TEntity : class, ITableEntity, new()
        {
            return AzureTableStorage<TEntity>.Create(connectionStringManager, tableName, log);
        }
    }
}