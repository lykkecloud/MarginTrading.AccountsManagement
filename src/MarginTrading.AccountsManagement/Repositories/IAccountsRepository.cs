using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAccountsRepository
    {
        Task AddAsync(Account account);
        
        Task<List<Account>> GetAllAsync(string clientId = null);

        [ItemCanBeNull]
        Task<Account> GetAsync(string clientId, string accountId);

        Task<Account> UpdateBalanceAsync(string clientId, string accountId, decimal amountDelta,
            bool changeLimit);

        Task<Account> UpdateTradingConditionIdAsync(string clientId, string accountId,
            string tradingConditionId);

        Task<Account> ChangeIsDisabledAsync(string clientId, string accountId, bool isDisabled);
    }
}