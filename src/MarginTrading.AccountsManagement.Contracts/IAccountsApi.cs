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
        Task<List<AccountContract>> List();

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
        /// Changes an account. Now the only editable fields are <see cref="AccountContract.TradingConditionId"/>
        /// and the <see cref="AccountContract.IsDisabled"/>, others are ignored.
        /// The <paramref name="account"/>.Id and <paramref name="account"/>.ClientId should match
        /// <paramref name="accountId"/> and <paramref name="clientId"/> 
        /// </summary>
        [Patch("/api/accounts/{clientId}/{accountId}")]
        Task<AccountContract> Change(string clientId, string accountId, [Body] AccountContract account);

        /// <summary>
        /// Manually charge client's account. Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [Post("/api/accounts/{clientId}/{accountId}/balance")]
        Task<AccountContract> ChargeManually(string clientId, string accountId,
            [Body] AccountChargeManuallyRequest request);
        
        /// <summary>
        /// Creates default accounts for client by trading conditions id.
        /// </summary>
        [Post("/api/accounts/{clientId}/create-default-accounts")]
        Task<List<AccountContract>> CreateDefaultAccounts(string clientId, 
            [Body] CreateDefaultAccountsRequest request);
        
        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        [Post("/api/accounts/create-for-base-asset")]
        Task<List<AccountContract>> CreateAccountsForBaseAsset(
            [Body] CreateAccountsForBaseAssetRequest request);
    }
}