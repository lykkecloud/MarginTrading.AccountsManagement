using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IAccountsRepository
    {
        Task AddAsync(IAccount account);
        
        Task<IReadOnlyList<IAccount>> GetAllAsync(string clientId = null, string search = null);
        
        Task<PaginatedResponse<IAccount>> GetByPagesAsync(string search = null, int? skip = null, int? take = null);

        [ItemCanBeNull]
        Task<IAccount> GetAsync(string clientId, string accountId);

        /// <summary>
        /// Updates the account if the operation has not yet been executed. 
        /// </summary>
        /// <returns>
        /// Account after update
        /// </returns>
        Task<IAccount> UpdateBalanceAsync(string operationId, [CanBeNull] string clientId, string accountId, 
            decimal amountDelta, bool changeLimit);

        Task<IAccount> UpdateTradingConditionIdAsync(string clientId, string accountId,
            string tradingConditionId);

        Task<IAccount> ChangeIsDisabledAsync(string clientId, string accountId, bool isDisabled);
    }
}