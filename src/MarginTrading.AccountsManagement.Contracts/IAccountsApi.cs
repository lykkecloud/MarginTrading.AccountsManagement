using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using Refit;

namespace MarginTrading.AccountsManagement.Contracts
{
    /// <summary>
    /// Manages accounts
    /// </summary>
    [PublicAPI]
    public interface IAccountsApi
    {
        /// <summary>
        /// Gets all accounts
        /// </summary>
        [Get("/api/accounts/")]
        Task<List<AccountContract>> List([Query] string search = null);

        /// <summary>
        /// Gets all accounts by <paramref name="clientId"/>
        /// </summary>
        [Get("/api/accounts/{clientId}")]
        Task<List<AccountContract>> GetByClient(string clientId);

        /// <summary>
        /// Gets account by clientId and accountId
        /// </summary>
        [Get("/api/accounts/{clientId}/{accountId}")]
        [ItemCanBeNull]
        Task<AccountContract> GetByClientAndId(string clientId, string accountId);

        /// <summary>
        /// Creates an account
        /// </summary>
        [Post("/api/accounts/{clientId}")]
        Task<AccountContract> Create(string clientId, [Body] CreateAccountRequest request);

        /// <summary>
        /// Changes an account.
        /// </summary>
        [Patch("/api/accounts/{clientId}/{accountId}")]
        Task<AccountContract> Change(string clientId, string accountId, [Body] ChangeAccountRequest request);

        /// <summary>
        /// Starts the operation of manually charging the client's account.
        /// Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [Post("/api/accounts/{clientId}/{accountId}/balance")]
        Task<string> BeginChargeManually(string clientId, string accountId, [Body] AccountChargeManuallyRequest request);

        /// <summary>
        /// Starts the operation of depositing funds to the client's account. Amount should be positive.
        /// </summary>
        [Post("/api/accounts/{clientId}/{accountId}/balance/deposit")]
        Task<string> BeginDeposit(string clientId, string accountId, [Body] AccountChargeRequest request);

        /// <summary>
        /// Starts the operation of withdrawing funds to the client's account. Amount should be positive.
        /// </summary>
        [Post("/api/accounts/{clientId}/{accountId}/balance/withdraw")]
        Task<string> BeginWithdraw(string clientId, string accountId, [Body] AccountChargeRequest request);

        /// <summary>
        /// Creates default accounts for client by trading condition id.
        /// </summary>
        [Post("/api/accounts/{clientId}/default-accounts")]
        Task<List<AccountContract>> CreateDefaultAccounts(string clientId, 
            [Body] CreateDefaultAccountsRequest request);

        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        [Post("/api/accounts/new-base-asset")]
        Task<List<AccountContract>> CreateAccountsForNewBaseAsset(
            [Body] CreateAccountsForBaseAssetRequest request);

        /// <summary>
        /// Reset account balance to default value (from settings)
        /// </summary>
        /// <returns></returns>
        [Post("/api/accounts/{clientId}/{accountId}/reset")]
        Task<AccountContract> Reset(string clientId, string accountId);
    }
}