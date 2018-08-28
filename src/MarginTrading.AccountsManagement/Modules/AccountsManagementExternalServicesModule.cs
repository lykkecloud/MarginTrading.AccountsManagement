using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.HttpClientGenerator;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.Backend.Contracts;
using MarginTrading.SettingsService.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.AccountsManagement.Modules
{
    internal class AccountsManagementExternalServicesModule : Module
    {
        private readonly IServiceCollection _services = new ServiceCollection();
        private readonly IReloadingManager<AppSettings> _settings;

        public AccountsManagementExternalServicesModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // todo register external services here
            var settingsServiceClientGenerator = HttpClientGenerator
                .BuildForUrl(_settings.CurrentValue.MarginTradingSettingsServiceClient.ServiceUrl)
                .Create();
            builder.RegisterInstance(settingsServiceClientGenerator.Generate<ITradingConditionsApi>());

            var mtCoreClientGenerator = HttpClientGenerator
                .BuildForUrl(_settings.CurrentValue.MtBackendServiceClient.ServiceUrl)
                .WithApiKey(_settings.CurrentValue.MtBackendServiceClient.ApiKey)
                .Create();
            builder.RegisterInstance(mtCoreClientGenerator.Generate<IOrdersApi>());
            builder.RegisterInstance(mtCoreClientGenerator.Generate<IPositionsApi>());
            
            builder.Populate(_services);
        }
    }
}