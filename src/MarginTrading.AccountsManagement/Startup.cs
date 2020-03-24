// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using Lykke.SettingsReader;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.ApiKey;
using Lykke.Snow.Common.Startup.Hosting;
using Lykke.Snow.Common.Startup.Log;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Modules;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Repositories.Implementation.SQL;
using MarginTrading.AccountsManagement.Services.Implementation;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MarginTrading.AccountsManagement
{
    public class Startup
    {
        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;

        private IHostingEnvironment Environment { get; }
        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        [CanBeNull] private ILog Log { get; set; }
        
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddEnvironmentVariables()
                .Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    });

                var appSettings = Configuration.LoadSettings<AppSettings>(
                    throwExceptionOnCheckError: !Configuration.NotThrowExceptionsOnServiceValidation());

                services.AddApiKeyAuth(appSettings.CurrentValue.MarginTradingAccountManagementServiceClient);

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", ServiceName + " API");
                    var contractsXmlPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, 
                        "MarginTrading.AccountsManagement.Contracts.xml");
                    options.IncludeXmlComments(contractsXmlPath);
                    options.OperationFilter<CustomOperationIdOperationFilter>();
                    if (!string.IsNullOrWhiteSpace(appSettings.CurrentValue.MarginTradingAccountManagementServiceClient?.ApiKey))
                    {
                        options.OperationFilter<ApiKeyHeaderOperationFilter>();
                    }
                });

                var builder = new ContainerBuilder();
                
                Log = CreateLog(Configuration, appSettings);

                services.AddSingleton<ILoggerFactory>(x => new WebHostLoggerFactory(Log));

                builder.RegisterModule(new AccountsManagementModule(appSettings, Log));
                builder.RegisterModule(new AccountsManagementExternalServicesModule(appSettings));
                builder.RegisterModule(new CqrsModule(appSettings.CurrentValue.MarginTradingAccountManagement.Cqrs, Log));

                builder.Populate(services);

                ApplicationContainer = builder.Build();
                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }
                          
#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => ex.ToString());
#else
                app.UseLykkeMiddleware(ServiceName, ex => new ErrorResponse {ErrorMessage = ex.Message});
#endif

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", "Main Swagger"));

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
                
                var provider = new AnnotationsBasedMetamodelProvider();

                EntityMetamodel.Configure(provider);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }


        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet receives and processes requests here
                await Program.Host.WriteLogsAsync(Environment, LogLocator.Log);

                await Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can receive and process requests here, so take care about it if you add logic here.
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }

                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't receive and process requests here, so you can destroy all resources

                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }

                throw;
            }
        }

        private static ILog CreateLog(IConfiguration configuration, IReloadingManager<AppSettings> settings)
        {
            var aggregateLogger = new AggregateLogger();
            var consoleLogger = new LogToConsole();
            
            aggregateLogger.AddLog(consoleLogger);

            if (settings.CurrentValue.MarginTradingAccountManagement.UseSerilog)
            {
                aggregateLogger.AddLog(new SerilogLogger(typeof(Startup).Assembly, configuration));
            }
            else if (settings.CurrentValue.MarginTradingAccountManagement.Db.StorageMode == StorageMode.SqlServer.ToString())
            {
                var sqlLogger = new LogToSql(new SqlLogRepository("AccountManagementLog",
                    settings.CurrentValue.MarginTradingAccountManagement.Db.LogsConnString));

                aggregateLogger.AddLog(sqlLogger);
            } 
            else if (settings.CurrentValue.MarginTradingAccountManagement.Db.StorageMode == StorageMode.Azure.ToString())
            {
                var dbLogConnectionStringManager = settings.Nested(x => x.MarginTradingAccountManagement.Db.LogsConnString);
                var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;
    
                if (string.IsNullOrEmpty(dbLogConnectionString))
                {
                    consoleLogger.WriteWarningAsync(nameof(Startup), nameof(CreateLog), "Table logger is not initialized").Wait();
                    return aggregateLogger;
                }
    
                if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                    throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");
    
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<Lykke.Logs.LogEntity>.Create(dbLogConnectionStringManager, "AccountManagementLog", consoleLogger),
                    consoleLogger);
    
                // Creating azure storage logger, which logs own messages to console log
                var azureStorageLogger = new LykkeLogToAzureStorage(persistenceManager, null, consoleLogger);
                
                azureStorageLogger.Start();
                
                aggregateLogger.AddLog(azureStorageLogger);
            }

            LogLocator.Log = aggregateLogger;

            return aggregateLogger;
        }
    }
}