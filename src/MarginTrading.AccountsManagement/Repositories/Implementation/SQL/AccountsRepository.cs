// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    internal class AccountsRepository : IAccountsRepository
    {
        #region SQL

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
                                                 "[IsDeleted] [bit] NOT NULL, " +
                                                 "[ModificationTimestamp] [DateTime] NOT NULL, " +
                                                 "[TemporaryCapital] [nvarchar] (MAX) NOT NULL, " +
                                                 "[LastExecutedOperations] [nvarchar] (MAX) NOT NULL, " +
                                                 "[AccountName] [nvarchar] (MAX), " +
                                                 "INDEX IX_{0} (ClientId, IsDeleted)" +
                                                 ");";

        private const string DeleteProcName = "DeleteAccountData";
        private readonly string DeleteProcCreateScript = $@"CREATE OR ALTER PROCEDURE [dbo].[{DeleteProcName}] (
        @AccountId NVARCHAR(128)
        )
        AS
            BEGIN
                SET NOCOUNT ON;
                BEGIN TRANSACTION
          
                DELETE [dbo].[AccountHistory] WHERE AccountId = @AccountId;
                DELETE [dbo].[OrdersHistory] WHERE AccountId = @AccountId;
                DELETE [dbo].[PositionsHistory] WHERE AccountId = @AccountId;
                DELETE [dbo].[Trades] WHERE AccountId = @AccountId;
                DELETE [dbo].[Deals] WHERE AccountId = @AccountId;
                DELETE [dbo].[Activities] WHERE AccountId = @AccountId;
          
                COMMIT TRANSACTION
            END;";

        #endregion SQL
        
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
                try
                {
                    conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
                    conn.Execute(DeleteProcCreateScript);
                }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(AccountsRepository), "Initialization", null, ex);
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

        public async Task<IReadOnlyList<IAccount>> GetAllAsync(string clientId = null, string search = null,
            bool showDeleted = false)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = "%" + search + "%";
            }
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var whereClause = "WHERE 1=1"
                                  + (string.IsNullOrWhiteSpace(clientId) ? "" : " AND ClientId = @clientId")
                                  + (string.IsNullOrWhiteSpace(search) ? "" : " AND Id LIKE @search")
                                  + (showDeleted ? "" : " AND IsDeleted = 0");
                var accounts = await conn.QueryAsync<AccountEntity>(
                    $"SELECT * FROM {TableName} {whereClause}", 
                    new { clientId, search });
                
                return accounts.ToList();
            }
        }

        public async Task<PaginatedResponse<IAccount>> GetByPagesAsync(string search = null, bool showDeleted = false,
            int? skip = null, int? take = null, bool isAscendingOrder = true)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = "%" + search + "%";
            }
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var whereClause = "WHERE 1=1"
                                  + (string.IsNullOrWhiteSpace(search) ? "" : " AND Id LIKE @search")
                                  + (showDeleted ? "" : " AND IsDeleted = 0");

                var paginationClause = $" ORDER BY [Id] {(isAscendingOrder ? "ASC" : "DESC")} OFFSET {skip ?? 0} ROWS FETCH NEXT {PaginationHelper.GetTake(take)} ROWS ONLY";
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT * FROM {TableName} {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}",
                    new {search});
                var accounts = (await gridReader.ReadAsync<AccountEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<IAccount>(
                    accounts, 
                    skip ?? 0, 
                    accounts.Count, 
                    !take.HasValue ? accounts.Count : totalCount
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

        public async Task<IAccount> DeleteAsync(string accountId)
        {
            return await GetAccountAndUpdate(accountId, a =>
            {
                a.IsDeleted = true;
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

        public async Task EraseAsync(string accountId)
        {
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                await conn.ExecuteAsync(
                    DeleteProcName,
                    new
                    {
                        AccountId = accountId,
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: _settings.Db.LongRunningSqlTimeoutSec);
            }
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
                        throw new ValidationException( $"Account with ID {accountId} does not exist");
                    }

                    if (account.IsDeleted)
                    {
                        throw new ValidationException($"Account with ID {accountId} is deleted");
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
            return new AccountEntity
            {
                Id = account.Id,
                ClientId = account.ClientId,
                TradingConditionId = account.TradingConditionId,
                BaseAssetId = account.BaseAssetId,
                Balance = account.Balance,
                WithdrawTransferLimit = account.WithdrawTransferLimit,
                LegalEntity = account.LegalEntity,
                IsDisabled = account.IsDisabled,
                IsWithdrawalDisabled = account.IsWithdrawalDisabled,
                ModificationTimestamp = account.ModificationTimestamp,
                TemporaryCapital = account.TemporaryCapital.ToJson(),
                LastExecutedOperations = account.LastExecutedOperations.ToJson(),
                AccountName = account.AccountName,
            };
        }
        
        #endregion
    }
}