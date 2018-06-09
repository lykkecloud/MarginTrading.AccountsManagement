using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Log;
using Dapper;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories.AzureServices;
using MarginTrading.AccountsManagement.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    internal class OperationExecutionInfoRepository : IOperationExecutionInfoRepository
    {
        private const string TableName = "OperationExecutionInfo";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[Id] [nvarchar] (64) NOT NULL PRIMARY KEY," +
                                                 "[OperationName] [nvarchar] (64) NOT NULL, " +
                                                 "[Version] [nvarchar] (64) NOT NULL, " +
                                                 "[Data] [nvarchar] (MAX) NOT NULL " +
                                                 ");";
        
        private static Type DataType => typeof(IOperationExecutionInfo<object>);
        private static readonly string GetColumns = string.Join(",", DataType.GetProperties().Select(x => x.Name));
        private static readonly string GetFields = string.Join(",", DataType.GetProperties().Select(x => "@" + x.Name));
        private static readonly string GetUpdateClause = string.Join(",",
            DataType.GetProperties().Select(x => "[" + x.Name + "]=@" + x.Name));

        private readonly IConvertService _convertService;
        private readonly AccountManagementSettings _settings;
        private readonly ILog _log;

        public OperationExecutionInfoRepository(IConvertService convertService, AccountManagementSettings settings, ILog log)
        {
            _convertService = convertService;
            _log = log;
            _settings = settings;
            
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(nameof(OperationExecutionInfoRepository), "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }
        
        public async Task<OperationExecutionInfo<TData>> GetOrAddAsync<TData>(string operationName, string operationId, Func<OperationExecutionInfo<TData>> factory) where TData : class
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                var operationInfo = await conn.QueryAsync<OperationExecutionInfoEntity>(
                    $"SELECT * FROM {TableName} WHERE Id = @operationId", new { operationId });

                if (operationId == null)
                {
                    await conn.ExecuteAsync(
                        $"insert into {TableName} ({GetColumns}) values ({GetFields})", Convert(factory()));
                }
                
                return operationInfo.Select(Convert<TData>).FirstOrDefault();
            }
        }

        public async Task<OperationExecutionInfo<TData>> GetAsync<TData>(string operationName, string id) where TData : class
        {
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
            {
                var operationInfo = await conn.QueryAsync<OperationExecutionInfoEntity>(
                    $"SELECT * FROM {TableName} WHERE Id = @id", new { id });

                return operationInfo == null || !operationInfo.Any() ? null : Convert<TData>(operationInfo.First());
            }
        }

        public async Task Save<TData>(OperationExecutionInfo<TData> executionInfo) where TData : class
        {
            var entity = Convert(executionInfo);
            
            using (var conn = new SqlConnection(_settings.Db.SqlConnectionString))
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
        }
        
        private static OperationExecutionInfo<TData> Convert<TData>(OperationExecutionInfoEntity entity)
            where TData : class
        {
            return new OperationExecutionInfo<TData>(
                version: entity.Version,
                operationName: entity.OperationName,
                id: entity.Id,
                data: entity.Data is string dataStr
                    ? JsonConvert.DeserializeObject<TData>(dataStr)
                    : ((JToken) entity.Data).ToObject<TData>());
        }

        private static OperationExecutionInfoEntity Convert<TData>(OperationExecutionInfo<TData> model)
            where TData : class
        {
            return new OperationExecutionInfoEntity
            {
                Id = model.Id,
                OperationName = model.OperationName,
                Version = model.Version,
                Data = model.Data.ToJson(),
            };
        }
    }
}