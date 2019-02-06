using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using AzureStorage;
using Common;
using Common.Log;
using Dapper;
using Lykke.Logs.MsSql.Extensions;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    internal class AccountsRepository : IAccountsRepository
    {
        private const string TableName = "MarginTradingAccounts";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Id] [nvarchar] (64) NOT NULL PRIMARY KEY, " +
                                                 "[ClientId] [nvarchar] (64) NOT NULL, " +
                                                 "[TradingConditionId] [nvarchar] (64) NOT NULL, " +
                                                 "[BaseAssetId] [nvarchar] (64) NOT NULL, " +
                                                 "[Balance] decimal (24, 12) NOT NULL, " +
                                                 "[WithdrawTransferLimit] decimal (24, 12) NOT NULL, " +
                                                 "[LegalEntity] [nvarchar] (64) NOT NULL, " +
                                                 "[IsDisabled] [bit] NOT NULL, " +
                                                 "[IsWithdrawalDisabled] [bit] NOT NULL, " +
                                                 "[ModificationTimestamp] [DateTime] NOT NULL, " +
                                                 "[TemporaryCapital] [nvarchar] (MAX) NOT NULL, " +
                                                 "[LastExecutedOperations] [nvarchar] (MAX) NOT NULL, " +
                                                 "INDEX IX_{0}_Client (ClientId)" +
                                                 ");";
        
        private static Type DataType => typeof(IAccount);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly AccountManagementSettings _settings;
        private readonly ILog _log;
        private const int MaxOperationsCount = 200;
        
        public AccountsRepository(IConvertService convertService, ISystemClock systemClock, 
            AccountManagementSettings settings, ILog log)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _log = log;
            _settings = settings;
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(AccountsRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(IAccount account)
        {
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})", Convert(account));
            }
        }

        public async Task<IReadOnlyList<IAccount>> GetAllAsync(string clientId = null, string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = "%" + search + "%";
            }
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var whereClause = "WHERE 1=1" +
                                  (string.IsNullOrWhiteSpace(clientId) ? "" : " AND ClientId = @clientId")
                    + (string.IsNullOrWhiteSpace(search) ? "" : " AND Id LIKE @search");
                var accounts = await conn.QueryAsync<AccountEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { clientId, search });
                
                return accounts.ToList();
            }
        }

        public async Task<PaginatedResponse<IAccount>> GetByPagesAsync(string search = null, int? skip = null, int? take = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = "%" + search + "%";
            }
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var whereClause = "WHERE 1=1"
                                  + (string.IsNullOrWhiteSpace(search) ? "" : " AND Id LIKE @search");

                var paginationClause = $" ORDER BY [Id] OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {search});
                var accounts = (await gridReader.ReadAsync<AccountEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<IAccount>(
                    contents: accounts, 
                    start: skip ?? 0, 
                    size: accounts.Count, 
                    totalSize: !take.HasValue ? accounts.Count : totalCount
                );
            }
        }

        public async Task<IAccount> GetAsync(string accountId)
        {
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var whereClause = "WHERE 1=1 "
                                  + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND Id = @accountId");
                var accounts = await conn.QueryAsync<AccountEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { accountId });
                
                return accounts.FirstOrDefault();
            }
        }

        public async Task<IAccount> UpdateBalanceAsync(string operationId, string accountId,
            decimal amountDelta, bool changeLimit)
        {
            return await GetAccountAndUpdate(accountId, account =>
            {
                if (TryUpdateOperationsList(operationId, account))
                {
                    account.Balance += amountDelta;

                    if (changeLimit)
                        account.WithdrawTransferLimit += amountDelta;
                    
                    account.ModificationTimestamp = _systemClock.UtcNow.UtcDateTime;
                }
            });
        }

        public async Task<IAccount> UpdateAccountAsync(string accountId, string tradingConditionId, bool? isDisabled,
            bool? isWithdrawalDisabled)
        {
            return await GetAccountAndUpdate(accountId, a =>
            {
                if (!string.IsNullOrEmpty(tradingConditionId))
                    a.TradingConditionId = tradingConditionId;

                if (isDisabled.HasValue)
                    a.IsDisabled = isDisabled.Value;

                if (isWithdrawalDisabled.HasValue)
                    a.IsWithdrawalDisabled = isWithdrawalDisabled.Value;
            });
        }

        public async Task<IAccount> UpdateAccountTemporaryCapitalAsync(string accountId,
            Func<string, List<TemporaryCapital>, TemporaryCapital, bool, List<TemporaryCapital>> handler,
            TemporaryCapital temporaryCapital, bool isAdd)
        {
            return await GetAccountAndUpdate(accountId, a =>
            {
                a.TemporaryCapital = handler(
                    accountId,
                    ((IAccount) a).TemporaryCapital,
                    temporaryCapital,
                    isAdd
                ).ToJson();
            });
        }

        public async Task<IAccount> RollbackTemporaryCapitalRevokeAsync(string accountId, 
            List<TemporaryCapital> revokedTemporaryCapital)
        {
            return await GetAccountAndUpdate(accountId, a =>
            {
                var result = ((IAccount) a).TemporaryCapital;

                result.AddRange(revokedTemporaryCapital.Where(x => result.All(r => r.Id != x.Id)));

                a.TemporaryCapital = result.ToJson();
            });
        }

        #region Private Methods

        private bool TryUpdateOperationsList(string operationId, AccountEntity a)
        {
            var lastExecutedOperations = _convertService.Convert<List<string>>(a.LastExecutedOperations);
            
            if (lastExecutedOperations.Contains(operationId))
                return false;
            
            lastExecutedOperations.Add(operationId);
            if (lastExecutedOperations.Count > MaxOperationsCount)
            {
                lastExecutedOperations.RemoveAt(0);
            }

            a.LastExecutedOperations = _convertService.Convert<string>(lastExecutedOperations);
            
            return true;
        }

        private async Task<IAccount> GetAccountAndUpdate(string accountId, Action<AccountEntity> handler)
        {
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();

                //Balance changing operation needs maximum level of isolation
                var transaction = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);

                try
                {
                    var account = await conn.QuerySingleOrDefaultAsync<AccountEntity>(
                        $"SELECT * FROM {TableName} WITH (UPDLOCK) WHERE Id = @accountId", new {accountId}, transaction);

                    if (account == null)
                    {
                        throw new ArgumentNullException(nameof(accountId), "Account does not exist");
                    }

                    handler(account);

                    await conn.ExecuteAsync(
                        $"update {TableName} set {GetUpdateClause} where Id=@Id", account, transaction);

                    transaction.Commit();

                    return account;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private AccountEntity Convert(IAccount account)
        {
            return _convertService.Convert<IAccount, AccountEntity>(account);
        }
        
        #endregion
    }
}