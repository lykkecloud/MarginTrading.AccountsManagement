using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ITradingConditionsService
    {
        Task<bool> IsTradingConditionExistsAsync(string tradingConditionId);
        
        Task<bool> IsAccountGroupExistsAsync(string tradingConditionId, string baseAssetId);

        Task<string> GetLegalEntityAsync(string tradingConditionId);

        Task<IEnumerable<string>> GetBaseAccountAssetsAsync(string tradingConditionId);
        
        Task<string> GetDefaultTradingConditionAsync();
    }
}