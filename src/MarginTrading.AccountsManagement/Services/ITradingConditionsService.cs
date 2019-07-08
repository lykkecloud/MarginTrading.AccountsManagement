// Copyright (c) 2019 Lykke Corp.

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ITradingConditionsService
    {
        Task<bool> IsTradingConditionExistsAsync(string tradingConditionId);
        
        Task<bool> IsBaseAssetExistsAsync(string tradingConditionId, string baseAssetId);

        [ItemCanBeNull] Task<string> GetLegalEntityAsync(string tradingConditionId);

        Task<IEnumerable<string>> GetBaseAccountAssetsAsync(string tradingConditionId);
        
        [ItemCanBeNull] Task<string> GetDefaultTradingConditionIdAsync();
    }
}