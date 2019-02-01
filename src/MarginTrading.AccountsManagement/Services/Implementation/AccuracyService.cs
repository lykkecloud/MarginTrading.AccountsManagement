using System;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.SettingsService.Contracts;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class AccuracyService : IAccuracyService
    {
        private readonly IAssetsApi _assetsApi;
        private readonly ILog _log;

        private const int MaxAccuracy = 16;

        public AccuracyService(
            IAssetsApi assetsApi,
            ILog log)
        {
            _assetsApi = assetsApi;
            _log = log;
        }
        
        public async Task<decimal> ToAccountAccuracy(decimal amount, string accountBaseAsset, string operationName)
        {
            var asset = await _assetsApi.Get(accountBaseAsset);

            var accuracy = asset?.Accuracy ?? MaxAccuracy;

            var roundedValue = Math.Round(amount, accuracy);

            if (roundedValue != amount)
            {
                await _log.WriteWarningAsync(nameof(AccuracyService), nameof(ToAccountAccuracy),
                    $"Amount was rounded to account base asset accuracy while starting [{operationName}] operation: [{amount}] -> [{roundedValue}].");
            }

            return roundedValue;
        }
    }
}