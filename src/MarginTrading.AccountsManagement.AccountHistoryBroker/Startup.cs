﻿using Autofac;
using Common.Log;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Services;
using AzureRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.AzureRepositories;
using SqlRepos = MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.SqlRepositories;
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
            IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            builder.RegisterInstance(new ConvertService())
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