using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common.Log;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.WindowsAzure.Storage.Table;

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

        public async Task AddAsync(IAccount account)
        {
            await _tableStorage.InsertAsync(Convert(account));
        }

        public async Task<IReadOnlyList<IAccount>> GetAllAsync(string clientId = null, string search = null)
        {
            var filter = string.IsNullOrEmpty(search)
                ? null
                : new Func<AccountEntity, bool>(account => account.Id.Contains(search));

            var accounts = string.IsNullOrEmpty(clientId)
                ? await _tableStorage.GetDataAsync(filter)
                : await _tableStorage.GetDataAsync(AccountEntity.GeneratePartitionKey(clientId), filter);

            return accounts.ToList();
        }

        public async Task<PaginatedResponse<IAccount>> GetByPagesAsync(string search = null, int? skip = null, int? take = null)
        {
            /*var data = (await _tableStorage.ExecuteQueryWithPaginationAsync(
                new TableQuery<AccountEntity>()
                {
                    //this condition might be not ok
                    FilterString = TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, search),
                    TakeCount = take,
                },
                new PagingInfo
                {
                    ElementCount = take, 
                    CurrentPage = skip / take
                })).ToList();
            */
            //TODO refactor before using azure impl
            var data = await GetAllAsync(clientId: null, search: search);
            
            return new PaginatedResponse<IAccount>(
                contents: take.HasValue ? data.OrderBy(x => x.Id).Skip(skip ?? 0).Take(PaginationHelper.GetTake(take)).ToList() : data,
                start: skip ?? 0,
                size: take ?? data.Count,
                totalSize: data.Count
            );
        }

        public async Task<IAccount> GetAsync(string accountId)
        {
            var account = (await _tableStorage.GetDataAsync(x => x.RowKey == accountId)).SingleOrDefault();

            return account;
        }

        public async Task<IAccount> UpdateBalanceAsync(string operationId, string accountId,
            decimal amountDelta, bool changeLimit)
        {
            AccountEntity account = null;

            var clientId = (await _tableStorage.GetDataAsync(x => x.RowKey == AccountEntity.GenerateRowKey(accountId)))
                .Single().ClientId;
            
            
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

            return account;
        }

        private static bool TryUpdateOperationsList(string operationId, AccountEntity a)
        {
            if (a.LastExecutedOperations.Contains(operationId))
                return false;
            
            a.LastExecutedOperations.Add(operationId);
            if(a.LastExecutedOperations.Count > MaxOperationsCount)
                a.LastExecutedOperations.RemoveAt(0);
            
            return true;
        }

        public async Task<IAccount> UpdateAccountAsync(string accountId, string tradingConditionId, bool? isDisabled,
            bool? isWithdrawalDisabled)
        {
            var pk = (await _tableStorage.GetDataRowKeyOnlyAsync(accountId)).Single().PartitionKey;

            var account = await _tableStorage.MergeAsync(pk,
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    if (!string.IsNullOrEmpty(tradingConditionId))
                        a.TradingConditionId = tradingConditionId;

                    if (isDisabled.HasValue)
                        a.IsDisabled = isDisabled.Value;

                    if (isWithdrawalDisabled.HasValue)
                        a.IsWithdrawalDisabled = isWithdrawalDisabled.Value;

                    return a;
                });

            return account;
        }

        public async Task<IAccount> UpdateAccountTemporaryCapitalAsync(string accountId,
            Func<string, List<TemporaryCapital>, TemporaryCapital, bool, List<TemporaryCapital>> handler,
            TemporaryCapital temporaryCapital, bool isAdd)
        {
            var pk = (await _tableStorage.GetDataRowKeyOnlyAsync(accountId)).Single().PartitionKey;
            
            var account = await _tableStorage.MergeAsync(pk,
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    a.TemporaryCapital = a.TemporaryCapital = handler(
                        accountId,
                        ((IAccount) a).TemporaryCapital,
                        temporaryCapital,
                        isAdd
                    );
                        
                    return a;
                });
            
            return account;
        }

        public async Task<IAccount> RollbackTemporaryCapitalRevokeAsync(string accountId, 
            List<TemporaryCapital> revokedTemporaryCapital)
        {
            var pk = (await _tableStorage.GetDataRowKeyOnlyAsync(accountId)).Single().PartitionKey;
            
            var account = await _tableStorage.MergeAsync(pk,
                AccountEntity.GenerateRowKey(accountId), a =>
                {
                    var result = ((IAccount) a).TemporaryCapital;
                
                    result.AddRange(revokedTemporaryCapital.Where(x => result.All(r => r.Id != x.Id)));

                    a.TemporaryCapital = result;
                    
                    return a;
                });
            
            return account;
        }

        private AccountEntity Convert(IAccount account)
        {
            return _convertService.Convert<IAccount, AccountEntity>(account,
                o => o.ConfigureMap(MemberList.Source).ForSourceMember(a => a.ModificationTimestamp, c => c.Ignore()));
        }
    }
}