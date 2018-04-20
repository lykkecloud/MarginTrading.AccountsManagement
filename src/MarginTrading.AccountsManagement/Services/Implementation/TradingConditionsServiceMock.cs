using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class TradingConditionsServiceMock : ITradingConditionsService
    {
        public Task<bool> IsTradingConditionExistsAsync(string tradingConditionId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsAccountGroupExistsAsync(string tradingConditionId, string baseAssetId)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetLegalEntityAsync(string tradingConditionId)
        {
            return Task.FromResult("Default");
        }

        public Task<IEnumerable<string>> GetBaseAccountAssetsAsync(string tradingConditionId)
        {
            return Task.FromResult(new[] {"USD"}.AsEnumerable());
        }

        public Task<string> GetDefaultTradingConditionAsync()
        {
            return Task.FromResult("DefaultTradingConditionId");
        }
    }
}