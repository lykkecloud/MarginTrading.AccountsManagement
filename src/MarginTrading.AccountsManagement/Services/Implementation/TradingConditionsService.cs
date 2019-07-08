// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.SettingsService.Contracts;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class TradingConditionsService : ITradingConditionsService
    {
        private readonly ITradingConditionsApi _tradingConditionsApi;

        public TradingConditionsService(ITradingConditionsApi tradingConditionsApi)
        {
            _tradingConditionsApi = tradingConditionsApi;
        }

        public async Task<bool> IsTradingConditionExistsAsync(string tradingConditionId)
        {
            return await _tradingConditionsApi.Get(tradingConditionId) != null;
        }

        public async Task<bool> IsBaseAssetExistsAsync(string tradingConditionId, string baseAssetId)
        {
            return (await _tradingConditionsApi.Get(tradingConditionId))?.BaseAssets.Contains(baseAssetId) == true;
        }

        public async Task<string> GetLegalEntityAsync(string tradingConditionId)
        {
            return (await _tradingConditionsApi.Get(tradingConditionId))?.LegalEntity;
        }

        public async Task<IEnumerable<string>> GetBaseAccountAssetsAsync(string tradingConditionId)
        {
            return (await _tradingConditionsApi.Get(tradingConditionId))?.BaseAssets;
        }

        public async Task<string> GetDefaultTradingConditionIdAsync()
        {
            return (await _tradingConditionsApi.List(true)).FirstOrDefault()
                .RequiredNotNull("Default trading condition").Id;
        }
    }
}