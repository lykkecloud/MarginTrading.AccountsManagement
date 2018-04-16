using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class TradingConditionsServiceMock : ITradingConditionsService
    {
        public Task<bool> IsTradingConditionExists(string tradingConditionId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsAccountGroupExists(string tradingConditionId, string baseAssetId)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetLegalEntity(string tradingConditionId)
        {
            return Task.FromResult("Default");
        }

        public Task<IEnumerable<string>> GetBaseAccountAssets(string tradingConditionId)
        {
            return Task.FromResult(new[] {"USD"}.AsEnumerable());
        }
    }
}