using AzureStorage;
using Common.Log;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.AccountsManagement.Repositories.AzureServices
{
    public interface IAzureTableStorageFactoryService
    {
        INoSQLTableStorage<TEntity> Create<TEntity>(IReloadingManager<string> connectionStringManager,
            string tableName, ILog log) where TEntity : class, ITableEntity, new();
    }
}