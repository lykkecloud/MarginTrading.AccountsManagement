using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/accounts")]
    public class AccountsController : Controller, IAccountsApi
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IConvertService _convertService;
        private readonly ISendBalanceCommandsService _sendBalanceCommandsService;

        public AccountsController(IAccountManagementService accountManagementService, IConvertService convertService,
            ISendBalanceCommandsService sendBalanceCommandsService)
        {
            _accountManagementService = accountManagementService;
            _convertService = convertService;
            _sendBalanceCommandsService = sendBalanceCommandsService;
        }

        /// <summary>
        /// Gets all accounts
        /// </summary>
        [HttpGet]
        [Route("")]
        public Task<List<AccountContract>> List()
        {
            return Convert(_accountManagementService.ListAsync());
        }

        /// <summary>
        /// Gets all accounts by <paramref name="clientId"/>
        /// </summary>
        [HttpGet]
        [Route("{clientId}")]
        public Task<List<AccountContract>> GetByClient(string clientId)
        {
            return Convert(_accountManagementService.GetByClientAsync(clientId));
        }

        /// <summary>
        /// Gets account by clientId and accountId
        /// </summary>
        [HttpGet]
        [Route("{clientId}/{accountId}")]
        public Task<AccountContract> GetByClientAndId(string clientId, string accountId)
        {
            return Convert(_accountManagementService.GetByClientAndIdAsync(clientId, accountId));
        }

        /// <summary>
        /// Creates an account
        /// </summary>
        [HttpPost]
        [Route("{clientId}")]
        public Task<AccountContract> Create(string clientId, [FromBody] CreateAccountRequest request)
        {
            return Convert(
                _accountManagementService.CreateAsync(clientId, request.TradingConditionId, request.BaseAssetId));
        }

        /// <summary>
        /// Changes an account.
        /// If the field is set, it will be changed, otherwise it will be ignored.
        /// </summary>
        [HttpPatch]
        [Route("{clientId}/{accountId}")]
        public async Task<AccountContract> Change(string clientId, string accountId,
            [FromBody] ChangeAccountRequest request)
        {
            Account result = null;

            if (!request.IsDisabled.HasValue && string.IsNullOrEmpty(request.TradingConditionId))
            {
                throw new ArgumentOutOfRangeException(nameof(request), "At least one parameter should be set");
            }

            if (request.IsDisabled.HasValue)
            {
                result = await _accountManagementService.SetDisabledAsync(clientId, accountId,
                    request.IsDisabled.Value);
            }

            if (!string.IsNullOrEmpty(request.TradingConditionId))
            {
                result = await _accountManagementService.SetTradingConditionAsync(clientId, accountId,
                    request.TradingConditionId);
            }

            return Convert(result);
        }

        /// <summary>
        /// Starts the operation of manually charging the client's account.
        /// Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance")]
        public Task BeginChargeManually(string clientId, string accountId, 
            [FromBody] AccountChargeManuallyRequest request)
        {
            return _sendBalanceCommandsService.ChargeManuallyAsync(clientId, accountId, request.AmountDelta,
                request.OperationId, request.Reason, "Api");
        }

        /// <summary>
        /// Starts the operation of depositing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/deposit")]
        public Task BeginDeposit(string clientId, string accountId, [FromBody] AccountChargeManuallyRequest request)
        {
            return _sendBalanceCommandsService.DepositAsync(clientId, accountId, request.AmountDelta,
                request.OperationId, request.Reason);
        }

        /// <summary>
        /// Starts the operation of withdrawing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/withdraw")]
        public Task BeginWithdraw(string clientId, string accountId, [FromBody] AccountChargeManuallyRequest request)
        {
            return _sendBalanceCommandsService.WithdrawAsync(clientId, accountId, request.AmountDelta,
                request.OperationId, request.Reason);
        }

        /// <summary>
        /// Reset account balance to default value (from settings)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("{clientId}/{accountId}/reset")]
        public Task<AccountContract> Reset(string clientId, string accountId)
        {
            return Convert(_accountManagementService.ResetAccountAsync(clientId, accountId));
        }

        /// <summary>
        /// Creates default accounts for client by trading condition id.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/default-accounts")]
        public Task<List<AccountContract>> CreateDefaultAccounts(string clientId, CreateDefaultAccountsRequest request)
        {
            return Convert(_accountManagementService.CreateDefaultAccountsAsync(clientId, request.TradingConditionId));
        }

        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        [HttpPost]
        [Route("new-base-asset")]
        public Task<List<AccountContract>> CreateAccountsForNewBaseAsset(CreateAccountsForBaseAssetRequest request)
        {
            return Convert(_accountManagementService.CreateAccountsForNewBaseAssetAsync(request.TradingConditionId,
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

        private AccountContract Convert(Account account)
        {
            return _convertService.Convert<Account, AccountContract>(account);
        }

        private Account Convert(AccountContract account)
        {
            return _convertService.Convert<AccountContract, Account>(account);
        }
    }
}