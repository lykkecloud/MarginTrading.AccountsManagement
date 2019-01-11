using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class AccountBalanceChangesRepository : IAccountBalanceChangesRepository
    {
        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly INoSQLTableStorage<AccountBalanceChangeEntity> _tableStorage;

        public AccountBalanceChangesRepository(IReloadingManager<AccountManagementSettings> settings, 
            ILog log,
            IConvertService convertService,
            ISystemClock systemClock,
            IAzureTableStorageFactoryService azureTableStorageFactoryService)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _tableStorage =
                azureTableStorageFactoryService.Create<AccountBalanceChangeEntity>(
                    settings.Nested(s => s.Db.ConnectionString), "AccountHistory", log);
        }

        public async Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, DateTime? @from = null,
            DateTime? to = null, AccountBalanceChangeReasonType? reasonType = null)
        {
            return (await _tableStorage.WhereAsync(accountId, from ?? DateTime.MinValue,
                    to?.Date.AddDays(1) ?? DateTime.MaxValue, ToIntervalOption.IncludeTo,
                    x => reasonType == null || x.ReasonType == reasonType.ToString()))
                .OrderByDescending(item => item.ChangeTimestamp).ToList();
        }

        public async Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, string eventSourceId)
        {
            return (await _tableStorage.GetDataAsync(x => x.PartitionKey == accountId
                                                          && (string.IsNullOrWhiteSpace(eventSourceId) ||
                                                              x.EventSourceId == eventSourceId)))
                .OrderByDescending(item => item.ChangeTimestamp).ToList();
        }

        public async Task<decimal> GetRealizedDailyPnl(string accountId)
        {
            return (await _tableStorage.WhereAsync(accountId,
                    //TODO rethink the way trading day's start & end are selected 
                    _systemClock.UtcNow.UtcDateTime.Date,
                    DateTime.MaxValue,
                    ToIntervalOption.IncludeTo,
                    x => x.ReasonType == AccountBalanceChangeReasonType.RealizedPnL.ToString()))
                .Sum(x => x.ChangeAmount);
        }

        public async Task AddAsync(IAccountBalanceChange change)
        {
            var entity = _convertService.Convert<AccountBalanceChangeEntity>(change);
            // ReSharper disable once RedundantArgumentDefaultValue
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.ChangeTimestamp,
                RowKeyDateTimeFormat.Iso);
        }

        private AccountBalanceChange Convert(AccountBalanceChangeEntity arg)
        {
            return _convertService.Convert<AccountBalanceChange>(arg);
        }
    }
}