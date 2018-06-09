using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using AzureStorage;
using Common.Log;
using Dapper;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    internal class AccountsRepository : IAccountsRepository
    {
        private const string TableName = "MarginTradingAccounts";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Id] [nvarchar] (64) NOT NULL PRIMARY KEY," +
                                                 "[ClientId] [nvarchar] (64) NOT NULL, " +
                                                 "[TradingConditionId] [nvarchar] (64) NOT NULL, " +
                                                 "[BaseAssetId] [nvarchar] (64) NOT NULL, " +
                                                 "[Balance] [float] NOT NULL, " +
                                                 "[WithdrawTransferLimit] [float] NOT NULL, " +
                                                 "[LegalEntity] [nvarchar] (64) NOT NULL, " +
                                                 "[IsDisabled] [bit] NOT NULL, " +
                                                 "[ModificationTimestamp] [DateTimeOffset] NOT NULL," +
                                                 "[LastExecutedOperations] [nvarchar] (MAX) NOT NULL" +
                                                 ");";
        
        private static Type DataType => typeof(IAccount);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly AccountManagementSettings _settings;
        private readonly ILog _log;
        private const int MaxOperationsCount = 200;
        
        public AccountsRepository(IConvertService convertService, AccountManagementSettings settings, ILog log)
        {
            _convertService = convertService;
            _log = log;
            _settings = settings;
            
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(AccountsRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(Account account)
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                await conn.ExecuteAsync(
                    $"insert into {TableName} ({GetColumns}) values ({GetFields})", Convert(account));
            }
        }

        public async Task<List<Account>> GetAllAsync(string clientId = null)
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                var whereClause = string.IsNullOrWhiteSpace(clientId) ? "" : "WHERE ClientId = @clientId";
                var accounts = await conn.QueryAsync<AccountEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { clientId });
                
                return accounts.Select(Convert).ToList();
            }
        }

        public async Task<Account> GetAsync(string clientId, string accountId)
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                var whereClause = (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(accountId) 
                    ? ""
                    : "WHERE 1=1 ")
                    + (string.IsNullOrWhiteSpace(accountId) ? "" : " AND Id = @accountId")
                    + (string.IsNullOrWhiteSpace(clientId) ? "" : " AND ClientId = @clientId");
                var accounts = await conn.QueryAsync<AccountEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { clientId, accountId });
                
                return accounts.Select(Convert).FirstOrDefault();
            }
        }

        public async Task<Account> UpdateBalanceAsync(string operationId, string clientId, string accountId, decimal amountDelta, bool changeLimit)
        {
            return await GetAccountAndUpdate(accountId, account =>
            {
                if (TryUpdateOperationsList(operationId, account))
                {
                    account.Balance += (double) amountDelta;

                    if (changeLimit)
                        account.WithdrawTransferLimit += amountDelta;
                }
            });
        }

        private static bool TryUpdateOperationsList(string operationId, IAccount a)
        {
            if (a.LastExecutedOperations.Contains(operationId))
                return false;
            
            a.LastExecutedOperations.Add(operationId);
            if(a.LastExecutedOperations.Count > MaxOperationsCount)
                a.LastExecutedOperations.RemoveAt(0);
            
            return true;
        }

        public async Task<Account> UpdateTradingConditionIdAsync(string clientId, string accountId, string tradingConditionId)
        {
            return await GetAccountAndUpdate(accountId, account => { account.TradingConditionId = tradingConditionId; });
        }

        public async Task<Account> ChangeIsDisabledAsync(string clientId, string accountId, bool isDisabled)
        {
            return await GetAccountAndUpdate(accountId, account => { account.IsDisabled = isDisabled; });
        }

        private async Task<Account> GetAccountAndUpdate(string accountId, Action<AccountEntity> handler)
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();

                var transaction = conn.BeginTransaction();

                try
                {
                    var account = await conn.QueryFirstAsync<AccountEntity>(
                        $"SELECT * FROM {TableName} WHERE Id = @accountId", new {accountId}, transaction);

                    if (account == null)
                    {
                        throw new ArgumentNullException(nameof(accountId), "Account does not exist");
                    }

                    handler(account);

                    await conn.ExecuteAsync(
                        $"update {TableName} set {GetUpdateClause} where Id=@Id", account, transaction);

                    transaction.Commit();
                    return Convert(account);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private Account Convert(AccountEntity entity)
        {
            return _convertService.Convert<AccountEntity, Account>(
                entity,
                o => o.ConfigureMap(MemberList.Destination).ForCtorParam(
                    "modificationTimestamp",
                    m => m.MapFrom(e => e.ModificationTimestamp)));
        }

        private AccountEntity Convert(Account account)
        {
            return _convertService.Convert<Account, AccountEntity>(account);
            //,o => o.ConfigureMap(MemberList.Source).ForSourceMember(a => a.ModificationTimestamp, c => c.Ignore()));
        }
    }
}