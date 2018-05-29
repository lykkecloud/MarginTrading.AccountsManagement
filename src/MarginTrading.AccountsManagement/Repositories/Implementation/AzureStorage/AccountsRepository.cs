using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class AccountsRepository : IAccountsRepository
    {
        private readonly IConvertService _convertService;
        private readonly INoSQLTableStorage<AccountEntity> _tableStorage;
        private const int MaxOperationsCount = 200;

        public AccountsRepository(IReloadingManager<AccountManagementSettings> settings, ILog log,
            IConvertService convertService, IAzureTableStorageFactoryService azureTableStorageFactoryService)
        {
            _convertService = convertService;

            _tableStorage = azureTableStorageFactoryService.Create<AccountEntity>(
                settings.Nested(s => s.Db.ConnectionString), "MarginTradingAccounts", log);
        }

        public async Task AddAsync(Account account)
        {
            await _tableStorage.InsertAsync(Convert(account));
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

        public async Task<Account> UpdateBalanceAsync(string operationId, string clientId, string accountId,
            decimal amountDelta, bool changeLimit)
        {
            AccountEntity account = null;
            await _tableStorage.InsertOrModifyAsync(AccountEntity.GeneratePartitionKey(clientId),
                AccountEntity.GenerateRowKey(accountId), 
                () => throw new InvalidOperationException($"Account {accountId} not exists"),
                a =>
                {
                    account = a;
                    if (!TryUpdateOperationsList(operationId, a))
                    {
                        return false;
                    }

                    a.Balance += amountDelta;

                    if (changeLimit)
                        a.WithdrawTransferLimit += amountDelta;

                    return true;
                });

            return Convert(account);
        }

        private static bool TryUpdateOperationsList(string operationId, AccountEntity a)
        {
            if (!a.LastExecutedOperations.Contains(operationId))
                return false;
            
            a.LastExecutedOperations.Add(operationId);
            if(a.LastExecutedOperations.Count > MaxOperationsCount)
                a.LastExecutedOperations.RemoveAt(0);
            
            return true;
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
            return _convertService.Convert<AccountEntity, Account>(
                entity,
                o => o.ConfigureMap(MemberList.Destination).ForCtorParam(
                    "modificationTimestamp",
                    m => m.MapFrom(e => e.Timestamp)));
        }

        private AccountEntity Convert(Account account)
        {
            return _convertService.Convert<Account, AccountEntity>(account,
                o => o.ConfigureMap(MemberList.Source).ForSourceMember(a => a.ModificationTimestamp, c => c.Ignore()));
        }
    }
}