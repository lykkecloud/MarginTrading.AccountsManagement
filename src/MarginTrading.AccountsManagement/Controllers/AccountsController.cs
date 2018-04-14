using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.DomainModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller, IAccountsApi
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IConvertService _convertService;

        public AccountsController(IAccountManagementService accountManagementService, IConvertService convertService)
        {
            _accountManagementService = accountManagementService;
            _convertService = convertService;
        }

        /// <summary>
        /// Gets all accounts
        /// </summary>
        [HttpGet]
        [Route("")]
        public Task<List<AccountContract>> List()
        {
            return Convert(_accountManagementService.List());
        }

        /// <summary>
        /// Gets all accounts by <paramref name="clientId"/>
        /// </summary>
        [HttpGet]
        [Route("{clientId}")]
        public Task<List<AccountContract>> GetByClient(string clientId)
        {
            return Convert(_accountManagementService.GetByClient(clientId));
        }

        /// <summary>
        /// Gets account by clientId and accountId
        /// </summary>
        [HttpGet]
        [Route("{clientId}/{accountId}")]
        public Task<AccountContract> GetByClientAndId(string clientId, string accountId)
        {
            return Convert(_accountManagementService.GetByClientAndId(clientId, accountId));
        }

        /// <summary>
        /// Creates an account
        /// </summary>
        [HttpPost]
        [Route("{clientId}")]
        public Task<AccountContract> Create(string clientId, [FromBody] CreateAccountRequest request)
        {
            return Convert(_accountManagementService.Create(clientId, request.TradingConditionId, request.BaseAssetId));
        }

        /// <summary>
        /// Changes an account. Now the only editable fields are <see cref="AccountContract.TradingConditionId"/>
        /// and the <see cref="AccountContract.IsDisabled"/>, others are ignored.
        /// The <paramref name="account"/>.Id and <paramref name="account"/>.ClientId should match
        /// <paramref name="accountId"/> and <paramref name="clientId"/> 
        /// </summary>
        [HttpPatch]
        [Route("{clientId}/{accountId}")]
        public Task<AccountContract> Change(string clientId, string accountId, [FromBody] AccountContract account)
        {
            account.Id.RequiredEqualsTo(accountId, nameof(account.Id));
            account.ClientId.RequiredEqualsTo(clientId, nameof(account.ClientId));
            return Convert(_accountManagementService.Change(Convert(account)));
        }

        /// <summary>
        /// Manually charge client's account. Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance")]
        public Task<AccountContract> ChargeManually(string clientId, string accountId,
            [FromBody] AccountChargeManuallyRequest request)
        {
            return Convert(_accountManagementService.ChargeManually(clientId, accountId, request.AmountDelta, request.Reason));
        }

        /// <summary>
        /// Creates default accounts for client by trading conditions id.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/create-default-accounts")]
        public Task<List<AccountContract>> CreateDefaultAccounts(string clientId, CreateDefaultAccountsRequest request)
        {
            return Convert(_accountManagementService.CreateDefaultAccounts(clientId, request.TradingConditionsId));
        }

        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        [HttpPost]
        [Route("create-for-base-asset")]
        public Task<List<AccountContract>> CreateAccountsForBaseAsset(CreateAccountsForBaseAssetRequest request)
        {
            return Convert(_accountManagementService.CreateAccountsForBaseAsset(request.TradingConditionId,
                request.BaseAssetId));
        }

        private async Task<List<AccountContract>> Convert(Task<List<Account>> accounts)
        {
            return (await accounts).Select(Convert).OrderBy(a => a.ClientId).ThenBy(a => a.BaseAssetId)
                .ThenBy(a => a.Id).ToList();
        }

        private async Task<AccountContract> Convert(Task<Account> account)
        {
            return Convert(await account);
        }

        public AccountContract Convert(Account account)
        {
            return _convertService.Convert<Account, AccountContract>(account);
        }

        public Account Convert(AccountContract account)
        {
            return _convertService.Convert<AccountContract, Account>(account);
        }
    }
}