using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.DomainModels;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class AccountManagementService : IAccountManagementService
    {
        public Task<List<Account>> List()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Account>> GetByClient(string clientId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Account> GetByClientAndId(string clientId, string accountId)
        {
            throw new System.NotImplementedException();
        }

        public Task<Account> Create(string clientId, string tradingConditionId, string baseAssetId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Account> Change(Account account)
        {
            var oldAccount = await GetByClientAndId(account.ClientId, account.Id);
            var newAccount = oldAccount.Apply(account.TradingConditionId, account.IsDisabled);
            _repository.Update(newAccount);
            return newAccount;
        }

        public Task<Account> ChargeManually(string clientId, string accountId, decimal amountDelta, string reason)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Account>> CreateDefaultAccounts(string clientId, string tradingConditionsId)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Account>> CreateAccountsForBaseAsset(string tradingConditionId, string baseAssetId)
        {
            throw new System.NotImplementedException();
        }
    }
}