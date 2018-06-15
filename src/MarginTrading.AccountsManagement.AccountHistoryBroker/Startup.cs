using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Logs;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories;
using AzureRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.AzureRepositories;
using SqlRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.SqlRepositories;
using MarginTrading.AccountsManagement.BrokerBase;
using MarginTrading.AccountsManagement.BrokerBase.Repositories;
using MarginTrading.AccountsManagement.BrokerBase.Services;
using MarginTrading.AccountsManagement.BrokerBase.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        protected override string ApplicationName => "AccountHistoryBroker";
        private const string LogTableName = "AccountHistoryBrokerLog";

        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, 
            IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterInstance(new ConvertService(cfg =>
                {
                    cfg.CreateMap<AccountBalanceChangeReasonType, string>().ConvertUsing(x => x.ToString());
                }))
                .As<IConvertService>().SingleInstance();

            if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer.ToString())
            {
                builder.RegisterType<SqlRepos.AccountHistoryRepository>()
                    .As<IAccountHistoryRepository>()
                    .SingleInstance();
            }
            else if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure.ToString())
            {
                builder.RegisterType<AzureRepos.AccountHistoryRepository>()
                    .As<IAccountHistoryRepository>()
                    .SingleInstance();
            }
        }

        protected override ILog CreateLog(IServiceCollection services, 
            IReloadingManager<DefaultBrokerApplicationSettings<Settings>> settings)
        {
            var logToConsole = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(logToConsole);
            
            if (settings.CurrentValue.MtBackend.MarginTradingSettings.Db.StorageMode == StorageMode.SqlServer.ToString())
            {
                var sqlLogger = new LogToSql(new SqlRepos.LogRepository(LogTableName,
                    settings.CurrentValue.MtBackend.MarginTradingSettings.Db.HistorySqlConnString));

                aggregateLogger.AddLog(sqlLogger);
            } 
            else if (settings.CurrentValue.MtBackend.MarginTradingSettings.Db.StorageMode == StorageMode.Azure.ToString())
            {
                // Creating azure storage logger, which logs own messages to concole log
                var dbLogConnectionString = settings.CurrentValue.MtBrokersLogs?.DbConnString;
                if (!string.IsNullOrEmpty(dbLogConnectionString) &&
                    !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
                {
                    var logToAzureStorage = services.UseLogToAzureStorage(
                        settings.Nested(s => s.MtBrokersLogs.DbConnString),
                        null,
                        ApplicationName + "Log",
                        aggregateLogger);

                    aggregateLogger.AddLog(logToAzureStorage);
                }
            }

            return aggregateLogger;
        }
    }
}