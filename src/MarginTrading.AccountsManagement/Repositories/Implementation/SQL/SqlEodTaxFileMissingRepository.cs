// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Snow.Common;
using Lykke.Snow.Common.Exceptions;
using MarginTrading.AccountsManagement.Dal.Common;
using Microsoft.Data.SqlClient;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    public class SqlEodTaxFileMissingRepository: SqlRepositoryBase, IEodTaxFileMissingRepository
    {
        private readonly ILog _log;
        
        public SqlEodTaxFileMissingRepository(string connectionString, ILog log) : base(connectionString)
        {
            _log = log;
            connectionString.InitializeSqlObject("dbo.TaxFileMissing.sql", log);
            ExecCreateOrAlter("dbo.addTaxFileMissing.sql");
            ExecCreateOrAlter("dbo.removeTaxFileMissing.sql");
            ExecCreateOrAlter("dbo.getTaxFileMissing.sql");
        }
        
        public async Task AddAsync(DateTime tradingDay)
        {
            try
            {
                await ExecuteNonQueryAsync("[dbo].[addTaxFileMissing]",
                    new[]
                    {
                        new SqlParameter("@TradingDate", SqlDbType.DateTime) {Value = tradingDay.Date}
                    });
            }
            catch (InsertionFailedException e)
            {
                await _log.WriteWarningAsync(
                    nameof(SqlEodTaxFileMissingRepository), 
                    nameof(AddAsync),
                    new {tradingDay}.ToJson(),
                    $"Couldn't insert new value into taxFileMissing table. Error message = {e.Message}");
            }
        }

        public async Task RemoveAsync(DateTime tradingDay)
        {
            try
            {
                await ExecuteNonQueryAsync("[dbo].[removeTaxFileMissing]",
                    new[] {new SqlParameter("@TradingDate", SqlDbType.DateTime) {Value = tradingDay.Date}});
            }
            catch (InsertionFailedException e)
            {
                await _log.WriteWarningAsync(
                    nameof(SqlEodTaxFileMissingRepository), 
                    nameof(RemoveAsync),
                    new {tradingDay}.ToJson(),
                    $"Couldn't delete from taxFileMissing table. Error message = {e.Message}");
            }
        }

        public async Task<IEnumerable<DateTime>> GetAllDaysAsync()
        {
            try
            {
                return await GetAllAsync("[dbo].[getTaxFileMissing]", null, Map);
            }
            catch (FormatException e)
            {
                await _log.WriteErrorAsync(
                    nameof(SqlEodTaxFileMissingRepository),
                    nameof(GetAllDaysAsync),
                    null,
                    e);
                
                return Enumerable.Empty<DateTime>();
            }
        }
        
        private DateTime Map(SqlDataReader reader)
        {
            return reader["TradingDate"] as DateTime? ?? throw new FormatException("Trading date column value can't be casted to datetime");
        }
    }
}