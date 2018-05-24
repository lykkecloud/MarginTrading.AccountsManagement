using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IAccountsRepository
    {
        Task AddAsync(Account account);
        
        Task<List<Account>> GetAllAsync(string clientId = null);

        [ItemCanBeNull]
        Task<Account> GetAsync(string clientId, string accountId);

        /// <summary>
        /// Updates the account if the operation has not yet been executed. 
        /// </summary>
        /// <returns>
        /// Account after update
        /// </returns>
        Task<Account> UpdateBalanceAsync(string operationId, string clientId, string accountId, decimal amountDelta,
            bool changeLimit);

        Task<Account> UpdateTradingConditionIdAsync(string clientId, string accountId,
            string tradingConditionId);

        Task<Account> ChangeIsDisabledAsync(string clientId, string accountId, bool isDisabled);
    }
}