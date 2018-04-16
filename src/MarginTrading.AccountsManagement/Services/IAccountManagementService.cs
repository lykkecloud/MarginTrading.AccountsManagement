using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.DomainModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAccountManagementService
    {
        
        #region Create 
        
        Task<Account> CreateAsync(string clientId, string tradingConditionId, string baseAssetId);
        
        /// <summary>
        /// Creates default accounts for client by trading condition id.
        /// </summary>
        Task<List<Account>> CreateDefaultAccountsAsync(string clientId, string tradingConditionId);
        
        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        Task<List<Account>> CreateAccountsForNewBaseAssetAsync(string tradingConditionId, string baseAssetId);
        
        #endregion
        
        
        #region Get
        
        Task<List<Account>> ListAsync();
        
        Task<List<Account>> GetByClientAsync(string clientId);
        
        Task<Account> GetByClientAndIdAsync(string clientId, string accountId);
        
        #endregion
        
        
        #region Modify
        
        Task<Account> SetTradingConditionAsync(string clientId, string accountId, string tradingConditionId);
        
        Task<Account> SetDisabledAsync(string clientId, string accountId, bool isDisabled);
        
        Task<Account> ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta, string reason);
        
        Task<Account> ResetAccountAsync(string clientId, string accountId);
        
        #endregion
        
    }
}