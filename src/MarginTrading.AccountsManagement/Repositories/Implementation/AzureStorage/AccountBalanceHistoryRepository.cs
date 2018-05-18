using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class AccountBalanceHistoryRepository : IAccountBalanceHistoryRepository
    {
        private readonly IConvertService _convertService;
        private readonly INoSQLTableStorage<AccountBalanceHistoryEntity> _tableStorage;

        public AccountBalanceHistoryRepository(IReloadingManager<AccountManagementSettings> settings, ILog log,
            IConvertService convertService, IAzureTableStorageFactoryService azureTableStorageFactoryService)
        {
            _convertService = convertService;
            _tableStorage =
                azureTableStorageFactoryService.Create<AccountBalanceHistoryEntity>(
                    settings.Nested(s => s.Db.ConnectionString), "AccountBalanceHistory", log);
        }

        public async Task<List<AccountBalanceHistory>> GetAsync(string[] accountIds, DateTime? from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(accountIds, from ?? DateTime.MinValue,
                    to?.Date.AddDays(1) ?? DateTime.MaxValue, ToIntervalOption.IncludeTo)).Select(Convert)
                .OrderByDescending(item => item.ChangeTimestamp).ToList();
        }

        public async Task AddAsync(AccountBalanceHistory history)
        {
            var entity = _convertService.Convert<AccountBalanceHistoryEntity>(history);
            // ReSharper disable once RedundantArgumentDefaultValue
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.ChangeTimestamp,
                RowKeyDateTimeFormat.Iso);
        }

        private AccountBalanceHistory Convert(AccountBalanceHistoryEntity arg)
        {
            return _convertService.Convert<AccountBalanceHistory>(arg);
        }
    }
}