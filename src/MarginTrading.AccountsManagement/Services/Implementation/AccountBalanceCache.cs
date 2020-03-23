// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Caching.Memory;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class AccountBalanceCache : IAccountBalanceCache
    {
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public AccountBalanceCache(
            IAccountBalanceChangesRepository accountBalanceChangesRepository, 
            IMemoryCache cache, 
            CacheSettings cacheSettings)
        {
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _cache = cache;
            _cacheSettings = cacheSettings;
        }

        public async Task<IReadOnlyList<IAccountBalanceChange>> GetByStartDateAsync(string accountId, DateTime @from)
        {
            if (string.IsNullOrEmpty(accountId))
                throw new ArgumentNullException(nameof(accountId));

            var key = GetCacheKey(accountId, @from);
            
            if (_cache.TryGetValue(key, out IReadOnlyList<IAccountBalanceChange> data))
            {
                return data;
            }

            var balanceChanges = await _accountBalanceChangesRepository.GetAsync(accountId, @from);

            _cache.Set(key, balanceChanges, _cacheSettings.ExpirationPeriod);
            
            return balanceChanges;
        }

        private string GetCacheKey(string accountId, DateTime dateFrom)
        {
            var dateText = dateFrom.ToString("yyyy-MM-dd");

            return $"{accountId}:{dateText}";
        }
    }
}