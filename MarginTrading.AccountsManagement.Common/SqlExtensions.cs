// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Common.Log;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MarginTrading.AccountsManagement.Dal.Common
{
    public static class SqlExtensions
    {
        public static void InitializeSqlObject(this string connectionString, string scriptFileName, ILog log = null)
        {
            var creationScript = FileExtensions.ReadFromFile(scriptFileName);
            
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Execute(creationScript);
                }
                catch (Exception ex)
                {
                    log?.WriteErrorAsync(typeof(SqlExtensions).FullName, nameof(InitializeSqlObject), 
                        scriptFileName, ex).Wait();
                    throw;
                }
            }
        }
    }
}