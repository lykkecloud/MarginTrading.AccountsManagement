﻿using System.Collections.Generic;
using System.Threading.Tasks;
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
            return (await _tradingConditionsApi.GetDefault())?.Id;
        }
    }
}