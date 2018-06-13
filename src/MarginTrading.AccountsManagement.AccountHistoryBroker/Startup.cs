using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories;
using AzureRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.AzureRepositories;
using SqlRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.SqlRepositories;
using MarginTrading.AccountsManagement.BrokerBase;
using MarginTrading.AccountsManagement.BrokerBase.Services;
using MarginTrading.AccountsManagement.BrokerBase.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        protected override string ApplicationName => "AccountHistoryBroker";

        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, 
            IReloadingManager<Settings> settings, ILog log, bool isLive)
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
    }
}