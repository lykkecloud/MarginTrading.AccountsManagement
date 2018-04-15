using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ITradingConditionsService
    {
        Task<bool> IsTradingConditionExists(string tradingConditionId);
        
        Task<bool> IsAccountGroupExists(string tradingConditionId, string baseAssetId);

        Task<string> GetLegalEntity(string tradingConditionId);

        Task<IEnumerable<string>> GetBaseAccountAssets(string tradingConditionId);
    }
}