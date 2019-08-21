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

        public AccountHistoryRepository(Settings settings, ILog log, IConvertService convertService)
        {
            _log = log;
            _settings = settings;
            _convertService = convertService;
            
            _settings.Db.ConnString.InitializeSqlObject("dbo.AccountHistory.sql", log);
            _settings.Db.ConnString.InitializeSqlObject("dbo.InsertAccountHistory.sql", log);
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
                    await conn.ExecuteAsync("[dbo].[SP_InsertAccountHistory]",
                        entity, 
                        transaction,
                        commandType: CommandType.StoredProcedure);
                    
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
        }
    }
}
