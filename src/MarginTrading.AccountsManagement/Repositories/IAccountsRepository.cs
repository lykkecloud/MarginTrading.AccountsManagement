using System;
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
        Task<IAccount> GetAsync(string accountId);

        /// <summary>
        /// Updates the account if the operation has not yet been executed. 
        /// </summary>
        /// <returns>
        /// Account after update
        /// </returns>
        Task<IAccount> UpdateBalanceAsync(string operationId, string accountId,
            decimal amountDelta, bool changeLimit);

        Task<IAccount> UpdateAccountAsync(string accountId, string tradingConditionId,
            bool? isDisabled, bool? isWithdrawalDisabled);

        Task<IAccount> UpdateAccountTemporaryCapitalAsync(string accountId, TemporaryCapital temporaryCapital, 
            bool addOrRemove);
    }
}