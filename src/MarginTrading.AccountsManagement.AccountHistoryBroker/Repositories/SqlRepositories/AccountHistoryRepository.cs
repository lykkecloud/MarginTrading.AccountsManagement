// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Extensions;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Services;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.SqlRepositories
{
    public class AccountHistoryRepository : IAccountHistoryRepository
    {
        private readonly Settings _settings;
        private readonly ILog _log;
        private readonly IConvertService _convertService;

        private static readonly string GetColumns =
            string.Join(",", typeof(IAccountHistory).GetProperties().Select(x => x.Name));

        private static readonly string GetFields =
            string.Join(",", typeof(IAccountHistory).GetProperties().Select(x => "@" + x.Name));

        public AccountHistoryRepository(Settings settings, ILog log, IConvertService convertService)
        {
            _log = log;
            _settings = settings;
            _convertService = convertService;
            
            _settings.Db.ConnString.InitializeSqlObject("dbo.AccountHistory.sql", log);
            _settings.Db.ConnString.InitializeSqlObject("dbo.SP_UpdateDealCommissionParamsOnAccountHistory.sql", log);
        }

        public async Task InsertAsync(IAccountHistory obj)
        {
            var entity = _convertService.Convert<AccountHistoryEntity>(obj);
            
            using (var conn = new SqlConnection(_settings.Db.ConnString))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    await conn.OpenAsync();
                }
                var transaction = conn.BeginTransaction(IsolationLevel.Serializable);
                
                try
                {
                    if (string.IsNullOrWhiteSpace(obj.EventSourceId) && new[]
                    {
                        AccountBalanceChangeReasonType.Swap,
                        AccountBalanceChangeReasonType.Commission,
                        AccountBalanceChangeReasonType.OnBehalf,
                        AccountBalanceChangeReasonType.Tax,
                    }.Contains(obj.ReasonType))
                    {
                        throw new Exception($"EventSourceId was null, with reason type {obj.ReasonType}");
                    }

                    await conn.ExecuteAsync($"INSERT INTO [dbo].[AccountHistory] ({GetColumns}) VALUES ({GetFields})",
                        entity,
                        transaction);
                    
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                    var msg = $"Error {ex.Message} \n" +
                           "Entity <AccountHistoryEntity>: \n" +
                           entity.ToJson();
                    _log?.WriteError(nameof(AccountHistoryRepository), nameof(InsertAsync), 
                        new Exception(msg));
                    
                    throw;
                }
            }
            
#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                try
                {
                    using (var conn = new SqlConnection(_settings.Db.ConnString))
                    {
                        await conn.ExecuteAsync("[dbo].[SP_UpdateDealCommissionParamsOnAccountHistory]",
                            new
                            {
                                ChangeAmount = obj.ChangeAmount,
                                ReasonType = obj.ReasonType.ToString(),
                                EventSourceId = obj.EventSourceId,
                            },
                            commandType: CommandType.StoredProcedure);
                    }
                }
                catch (Exception exception)
                {
                    await _log?.WriteErrorAsync(nameof(AccountHistoryRepository), nameof(InsertAsync), 
                        new Exception($"Failed to calculate commissions for eventSourceId {obj.EventSourceId} with reasonType {obj.ReasonType}, skipping.", 
                            exception));
                }        
            });
        }
    }
}
