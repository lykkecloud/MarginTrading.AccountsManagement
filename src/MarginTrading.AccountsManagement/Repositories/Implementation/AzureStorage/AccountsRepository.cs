using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.DomainModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly IConvertService _convertService;
        private readonly INoSQLTableStorage<AccountEntity> _tableStorage;

        public AccountsRepository(IReloadingManager<AccountManagementSettings> settings, ILog log,
            IConvertService convertService, IAzureTableStorageFactoryService azureTableStorageFactoryService)
        {
            _convertService = convertService;

            _tableStorage = azureTableStorageFactoryService.Create<AccountEntity>(
                settings.Nested(s => s.Db.ConnectionString), "MarginTradingAccounts", log);
        }

        public async Task AddAsync(Account account)
        {
            var entity =
                _convertService.Convert<Account, AccountEntity>(account,
                    o => o.ConfigureMap(MemberList.Source));
            entity.PartitionKey = AccountEntity.GeneratePartitionKey(account.ClientId);
            entity.RowKey = AccountEntity.GenerateRowKey(account.Id);

            await _tableStorage.InsertAsync(entity);
        }

        public async Task<List<Account>> GetAllAsync(string clientId = null)
        {
            var accounts = string.IsNullOrEmpty(clientId)
                ? await _tableStorage.GetDataAsync()
                : await _tableStorage.GetDataAsync(AccountEntity.GeneratePartitionKey(clientId));

            return accounts.Select(Convert).ToList();
        }

        public async Task<Account> GetAsync(string clientId, string accountId)
        {
            var account = await _tableStorage.GetDataAsync(AccountEntity.GeneratePartitionKey(clientId),
                AccountEntity.GenerateRowKey(accountId));

            return Convert(account);
        }

        public async Task<Account> UpdateBalanceAsync(string clientId, string accountId, decimal amount, bool changeLimit)
        {
            var account = await _tableStorage.MergeAsync(AccountEntity.GeneratePartitionKey(clientId),
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    a.Balance += amount;

                    if (changeLimit)
                        a.WithdrawTransferLimit += amount;

                    return a;
                });
            
            return Convert(account);
        }

        public async Task<Account> UpdateTradingConditionIdAsync(string clientId, string accountId, string tradingConditionId)
        {
            var account = await _tableStorage.MergeAsync(AccountEntity.GeneratePartitionKey(clientId),
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    a.TradingConditionId = tradingConditionId;

                    return a;
                });
            
            return Convert(account);
        }

        public async Task<Account> ChangeIsDisabledAsync(string clientId, string accountId, bool isDisabled)
        {
            var account = await _tableStorage.MergeAsync(AccountEntity.GeneratePartitionKey(clientId),
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    a.IsDisabled = isDisabled;

                    return a;
                });
            
            return Convert(account);
        }

        private Account Convert(AccountEntity entity)
        {
            return _convertService.Convert<AccountEntity, Account>(entity);
        }
    }
}