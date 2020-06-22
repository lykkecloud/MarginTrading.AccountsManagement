// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    public class AccountBalanceChangesRepository : IAccountBalanceChangesRepository
    {
        private const string TableName = "AccountHistory";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Oid] [bigint] NOT NULL IDENTITY (1,1) PRIMARY KEY, " +
                                                 "[Id] [nvarchar] (64) NOT NULL UNIQUE, " +
                                                 "[AccountId] [nvarchar] (64) NOT NULL, " +
                                                 "[ChangeTimestamp] [datetime] NOT NULL, " +
                                                 "[ClientId] [nvarchar] (64) NOT NULL, " +
                                                 "[ChangeAmount] decimal (24, 12) NOT NULL, " +
                                                 "[Balance] decimal (24, 12) NOT NULL, " +
                                                 "[WithdrawTransferLimit] decimal (24, 12) NOT NULL, " +
                                                 "[Comment] [nvarchar] (MAX) NULL, " +
                                                 "[ReasonType] [nvarchar] (64) NULL, " +
                                                 "[EventSourceId] [nvarchar] (64) NULL, " +
                                                 "[LegalEntity] [nvarchar] (64) NULL, " +
                                                 "[AuditLog] [nvarchar] (MAX) NULL, " +
                                                 "[Instrument] [nvarchar] (64) NULL, " +
                                                 "[TradingDate] [datetime] NULL, " +
                                                 "INDEX IX_{0}_Base (Id, AccountId, ChangeTimestamp, EventSourceId, ReasonType)" +
                                                 ");";
        
        private static Type DataType => typeof(IAccountBalanceChange);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly AccountManagementSettings _settings;
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;
        
        public AccountBalanceChangesRepository(IConvertService convertService, 
            AccountManagementSettings settings, 
            ILog log,
            ISystemClock systemClock)
        {
            _convertService = convertService;
            _settings = settings;
            _log = log;
            _systemClock = systemClock;
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(AccountBalanceChangesRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task<PaginatedResponse<IAccountBalanceChange>> GetByPagesAsync(string accountId,
            DateTime? @from = null, DateTime? to = null, AccountBalanceChangeReasonType[] reasonTypes = null, 
            string assetPairId = null, int? skip = null, int? take = null, bool isAscendingOrder = true)
        {
            take = PaginationHelper.GetTake(take);

            var whereClause = "WHERE 1=1 " 
                                + (!string.IsNullOrWhiteSpace(accountId) ? " AND AccountId=@accountId" : "")
                                + (from != null ? " AND ChangeTimestamp > @from" : "")
                                + (to != null ? " AND ChangeTimestamp < @to" : "")
                                + (reasonTypes != null && reasonTypes.Any() ? " AND ReasonType IN @types" : "")
                                + (!string.IsNullOrWhiteSpace(assetPairId) ? " AND Instrument=@assetPairId" : "");

            var sorting = isAscendingOrder ? "ASC" : "DESC";
            var paginationClause = $" ORDER BY [ChangeTimestamp] {sorting} OFFSET {skip ?? 0} ROWS FETCH NEXT {take} ROWS ONLY";

            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var gridReader = await conn.QueryMultipleAsync(
                    $"SELECT {GetColumns} FROM {TableName} WITH (NOLOCK) {whereClause} {paginationClause}; SELECT COUNT(*) FROM {TableName} {whereClause}", new
                    {
                        accountId, 
                        from, 
                        to, 
                        types = reasonTypes?.Select(x => x.ToString()),
                        assetPairId
                    });

                var contents = (await gridReader.ReadAsync<AccountBalanceChangeEntity>()).ToList();
                var totalCount = await gridReader.ReadSingleAsync<int>();

                return new PaginatedResponse<IAccountBalanceChange>(
                    contents, 
                    skip ?? 0, 
                    contents.Count, 
                    totalCount
                );
            }
        }
        
        public async Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, DateTime? @from = null,
            DateTime? to = null, AccountBalanceChangeReasonType? reasonType = null, bool filterByTradingDay = false)
        {
            var timeFilterField = filterByTradingDay ? "TradingDate" : "ChangeTimestamp";
            var whereClause = "WHERE 1=1 " + (!string.IsNullOrWhiteSpace(accountId) ? " AND AccountId=@accountId" : "")
                                       + (from != null ? $" AND {timeFilterField} >= @from" : "")
                                       + (to != null ? $" AND {timeFilterField} < @to" : "")
                                       + (reasonType != null ? " AND ReasonType = @reasonType" : "");
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var data = await conn.QueryAsync<AccountBalanceChangeEntity>(
                    $"SELECT {GetColumns} FROM {TableName} WITH (NOLOCK) {whereClause}", new
                    {
                        accountId, 
                        from, 
                        to, 
                        reasonType = reasonType.ToString(),
                    });

                return data.ToList();
            }
        }

        public async Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, string eventSourceId)
        {
            var whereClause = "WHERE AccountId=@accountId "
                + (string.IsNullOrWhiteSpace(eventSourceId) ? "" : "AND EventSourceId=@eventSourceId");
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                var data = await conn.QueryAsync<AccountBalanceChangeEntity>(
                    $"SELECT {GetColumns} FROM {TableName} WITH (NOLOCK) {whereClause}", 
                    new { accountId, eventSourceId });

                return data.ToList();
            }
        }

        public async Task<decimal> GetCompensationsForToday(string accountId)
        {
            var whereClause = "WHERE AccountId=@accountId"
                              + " AND ChangeTimestamp > @from"
                              + " AND ReasonType IN @reasonTypes";

            var reasonTypes = new[]
            {
                AccountBalanceChangeReasonType.CompensationPayments.ToString()
            };

            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                return await conn.QuerySingleAsync<decimal?>(
                    $"SELECT SUM(ChangeAmount) FROM {TableName} WITH (NOLOCK) {whereClause}", new
                    {
                        accountId,
                        from = _systemClock.UtcNow.UtcDateTime.Date,
                        reasonTypes,
                    }) ?? 0;
            }
        }

        public async Task AddAsync(IAccountBalanceChange change)
        {
            var entity = _convertService.Convert<AccountBalanceChangeEntity>(change);
            
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                try
                {
                    try
                    {
                        await conn.ExecuteAsync(
                            $"insert into {TableName} ({GetColumns}) values ({GetFields})", entity);
                    }
                    catch (SqlException)
                    {
                        await conn.ExecuteAsync(
                            $"update {TableName} set {GetUpdateClause} where Id=@Id", entity); 
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Error {ex.Message} \n" +
                              "Entity <AccountBalanceChangeEntity>: \n" +
                              entity.ToJson();
                    await _log.WriteWarningAsync(nameof(AccountBalanceChangesRepository), nameof(AddAsync), null, msg);
                    throw new Exception(msg);
                }
            }
        }

        public async Task<decimal> GetBalanceAsync(string accountId, DateTime date)
        {
            using (var conn = new SqlConnection(_settings.Db.ConnectionString))
            {
                return await conn.ExecuteScalarAsync<decimal>(
                    $"SELECT TOP 1 Balance FROM {TableName} WHERE AccountId=@accountId AND ChangeTimestamp < @date ORDER BY ChangeTimestamp DESC",
                    new
                    {
                        accountId,
                        date = date.Date.AddDays(1),
                    });
            }
        }
    }
}