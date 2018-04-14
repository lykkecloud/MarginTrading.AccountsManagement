using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.DomainModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAccountManagementService
    {
        Task<List<Account>> List();
        Task<List<Account>> GetByClient(string clientId);
        Task<Account> GetByClientAndId(string clientId, string accountId);
        Task<Account> Create(string clientId, string tradingConditionId, string baseAssetId);
        Task<Account> Change(Account account);
        Task<Account> ChargeManually(string clientId, string accountId, decimal amountDelta, string reason);
        Task<List<Account>> CreateDefaultAccounts(string clientId, string tradingConditionsId);
        Task<List<Account>> CreateAccountsForBaseAsset(string tradingConditionId, string baseAssetId);
    }
}