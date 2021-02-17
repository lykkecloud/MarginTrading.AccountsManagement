using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Snow.Mdm.Contracts.BrokerFeatures;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Polly;

namespace MarginTrading.AccountsManagement.HostedServices
{
    public class ProductComplexityHostedService : BackgroundService
    {
        private readonly IFeatureManager _featureManager;
        private readonly ILog _log;
        private readonly AccountManagementSettings _settings;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IComplexityWarningRepository _complexityWarningRepository;
        
        public ProductComplexityHostedService(IFeatureManager featureManager, ILog log, AccountManagementSettings settings, IAccountManagementService accountManagementService, IComplexityWarningRepository complexityWarningRepository)
        {
            _featureManager = featureManager;
            _log = log;
            _settings = settings;
            _accountManagementService = accountManagementService;
            _complexityWarningRepository = complexityWarningRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await _featureManager.IsEnabledAsync(BrokerFeature.ProductComplexityWarning))
            {
                await _log.WriteInfoAsync(nameof(ProductComplexityHostedService),
                    nameof(Run), 
                    $"Feature {BrokerFeature.ProductComplexityWarning} is disabled. " +
                          $"{nameof(ProductComplexityHostedService)}.{nameof(Run)} will not be executed");

                return;
            }

            await _log.WriteInfoAsync(nameof(ProductComplexityHostedService),
                nameof(Run),
                $"Feature {BrokerFeature.ProductComplexityWarning} is enabled. " +
                $"{nameof(ProductComplexityHostedService)}.{nameof(Run)} will be executed");

            var retryForever = Policy.Handle<Exception>()
                .RetryForeverAsync(onRetry: async ex =>
                {
                    await _log.WriteErrorAsync(nameof(ProductComplexityHostedService), nameof(Run), ex);
                });

            while (!stoppingToken.IsCancellationRequested)
            {
                await retryForever.ExecuteAsync(Run, stoppingToken);
                await Task.Delay(_settings.ComplexityWarningExpirationCheckPeriod, stoppingToken);
            }
        }

        private async Task Run(CancellationToken stoppingToken)
        {
            var expirationTimestamp = DateTime.UtcNow.Subtract(_settings.ComplexityWarningExpiration);

            var expiredAccounts = await _complexityWarningRepository.GetExpired(expirationTimestamp);

            foreach (var acc in expiredAccounts)
            {
                stoppingToken.ThrowIfCancellationRequested();

                await _log.WriteInfoAsync(nameof(ProductComplexityHostedService), nameof(Run),
                            $"Product complexity confirmation expired for account {acc.AccountId}. " +
                                 $"{nameof(_settings.ComplexityWarningExpiration)} : {_settings.ComplexityWarningExpiration}." +
                                 $"{nameof(expirationTimestamp)} : {expirationTimestamp}." +
                                 $"{nameof(acc.SwitchedToFalseAt)} : {acc.SwitchedToFalseAt}." +
                                 $"Resetting {nameof(IAccount.AdditionalInfo.ShouldShowProductComplexityWarning)} flag to true");

                acc.ResetConfirmation();
                await _accountManagementService.UpdateComplexityWarningFlag(acc.AccountId, shouldShowProductComplexityWarning: true);

                await _complexityWarningRepository.Save(acc);
            }
        }
    }
}
