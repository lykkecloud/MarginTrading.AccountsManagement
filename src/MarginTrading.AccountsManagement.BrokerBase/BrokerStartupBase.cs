using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Lykke.SlackNotifications;
using MarginTrading.AccountsManagement.BrokerBase.Services;
using MarginTrading.AccountsManagement.BrokerBase.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MarginTrading.AccountsManagement.BrokerBase
{
    public abstract class BrokerStartupBase<TApplicationSettings, TSettings>
        where TApplicationSettings : class, IBrokerApplicationSettings<TSettings>
        where TSettings: BrokerSettingsBase
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public ILog Log { get; private set; }

        protected abstract string ApplicationName { get; }

        protected BrokerStartupBase(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddDevJson(env)
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Error)
                .AddDebug(LogLevel.Warning);

            services.AddSingleton(loggerFactory);
            services.AddLogging();
            services.AddSingleton(Configuration);
            services.AddMvc();

            var applicationSettings = Configuration.LoadSettings<TApplicationSettings>()
                .Nested(s =>
                {
                    var settings = s.MtBackend.MarginTradingSettings;
                    if (!string.IsNullOrEmpty(Configuration["Env"]))
                    {
                        settings.Env = Configuration["Env"];
                    }
                    SetSettingValues(settings, Configuration);
                    return s;
                });

            var builder = new ContainerBuilder();
            RegisterServices(services, applicationSettings, builder);
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        protected virtual void SetSettingValues(TSettings source, IConfigurationRoot configuration)
        {
            //if needed TSetting properties may be set
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            app.UseMvc();

            var applications = app.ApplicationServices.GetServices<IBrokerApplication>();

            appLifetime.ApplicationStarted.Register(async () =>
            {
                foreach (var application in applications)
                {
                    application.Run();
                }
                
                await Log.WriteMonitorAsync("", "", $"Started");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                foreach (var application in applications)
                {
                    application.StopApplication();
                }
            });

            appLifetime.ApplicationStopped.Register(async () =>
            {
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", $"Terminating");
                }
                
                ApplicationContainer.Dispose();
            });
        }

        protected abstract void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, IReloadingManager<TSettings> settings, ILog log);

        protected abstract ILog CreateLog(IServiceCollection services, IReloadingManager<TApplicationSettings> settings);
        
        private void RegisterServices(IServiceCollection services, IReloadingManager<TApplicationSettings> applicationSettings,
            ContainerBuilder builder)
        {
            Log = CreateLog(services, applicationSettings);
            builder.RegisterInstance(Log).As<ILog>().SingleInstance();
            builder.RegisterInstance(applicationSettings).AsSelf().SingleInstance();

            var settings = applicationSettings.Nested(s => s.MtBackend.MarginTradingSettings);
            builder.RegisterInstance(settings).AsSelf().SingleInstance();
            builder.RegisterInstance(settings.CurrentValue).AsSelf().SingleInstance();

            builder.RegisterInstance(new CurrentApplicationInfo(PlatformServices.Default.Application.ApplicationVersion,
                ApplicationName
            )).AsSelf().SingleInstance();

            RegisterCustomServices(services, builder, settings, Log);
            builder.Populate(services);
        }
    }
}
