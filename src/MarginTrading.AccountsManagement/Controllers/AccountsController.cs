using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Refit;

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
        public Task<List<AccountContract>> List([FromQuery] string search = null)
        {
            return Convert(_accountManagementService.ListAsync(search));
        }

        /// <summary>
        /// Gets all accounts by <paramref name="clientId"/>
        /// </summary>
        [HttpGet]
        [Route("{clientId}")]
        public Task<List<AccountContract>> GetByClient([NotNull] string clientId)
        {
            return Convert(_accountManagementService.GetByClientAsync(
                clientId.RequiredNotNullOrWhiteSpace(nameof(clientId))));
        }

        /// <summary>
        /// Gets account by clientId and accountId
        /// </summary>
        [HttpGet]
        [Route("{clientId}/{accountId}")]
        public Task<AccountContract> GetByClientAndId([NotNull] string clientId, [NotNull] string accountId)
        {
            return Convert(_accountManagementService.GetByClientAndIdAsync(
                clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                accountId.RequiredNotNullOrWhiteSpace(nameof(accountId))));
        }

        /// <summary>
        /// Creates an account
        /// </summary>
        [HttpPost]
        [Route("{clientId}")]
        public Task<AccountContract> Create([NotNull] string clientId, [FromBody][NotNull] CreateAccountRequest request)
        {
            return Convert(
                _accountManagementService.CreateAsync(
                    clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                    request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                    request.TradingConditionId,
                    request.BaseAssetId.RequiredNotNullOrWhiteSpace(nameof(request.BaseAssetId))));
        }

        /// <summary>
        /// Changes an account.
        /// If the field is set, it will be changed, otherwise it will be ignored.
        /// </summary>
        [HttpPatch]
        [Route("{clientId}/{accountId}")]
        public async Task<AccountContract> Change([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] ChangeAccountRequest request)
        {
            IAccount result = null;

            if (!request.IsDisabled.HasValue && string.IsNullOrEmpty(request.TradingConditionId))
            {
                throw new ArgumentOutOfRangeException(nameof(request), "At least one parameter should be set");
            }

            if (request.IsDisabled.HasValue)
            {
                result = await _accountManagementService.SetDisabledAsync(
                    clientId,
                    accountId,
                    request.IsDisabled.Value);
            }

            if (!string.IsNullOrEmpty(request.TradingConditionId))
            {
                result = await _accountManagementService.SetTradingConditionAsync(
                    clientId,
                    accountId,
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
        public async Task<string> BeginChargeManually([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeManuallyRequest request)
        {
            await ValidateClientAndAccountId(clientId, accountId);

            return await _sendBalanceCommandsService.ChargeManuallyAsync(
                clientId: clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: request.AmountDelta.RequiredNotEqualsTo(default(decimal), nameof(request.AmountDelta)),
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)),
                source: "Api",
                auditLog: request.AdditionalInfo,
                type: request.ReasonType.ToType<AccountBalanceChangeReasonType>(),
                eventSourceId: request.EventSourceId, 
                assetPairId: request.AssetPairId, 
                tradingDate: request.TradingDay ?? DateTime.UtcNow);
        }

        /// <summary>
        /// Starts the operation of depositing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/deposit")]
        public async Task<string> BeginDeposit([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            await ValidateClientAndAccountId(clientId, accountId);
            
            return await _sendBalanceCommandsService.DepositAsync(
                clientId: clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: request.AmountDelta.RequiredGreaterThan(default(decimal), nameof(request.AmountDelta)),
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)),
                auditLog: request.AdditionalInfo);
        }

        /// <summary>
        /// Starts the operation of withdrawing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/withdraw")]
        public async Task<string> BeginWithdraw([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            await ValidateClientAndAccountId(clientId, accountId);
            
            return await _sendBalanceCommandsService.WithdrawAsync(
                clientId: clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: request.AmountDelta.RequiredGreaterThan(default(decimal), nameof(request.AmountDelta)),
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)),
                auditLog: request.AdditionalInfo);
        }

        /// <summary>
        /// Reset account balance to default value (from settings)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("{clientId}/{accountId}/reset")]
        public Task<AccountContract> Reset([NotNull] string clientId, [NotNull] string accountId)
        {
            return Convert(_accountManagementService.ResetAccountAsync(
                clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)),
                accountId.RequiredNotNullOrWhiteSpace(nameof(accountId))));
        }

        /// <summary>
        /// Get account statistics for the current trading day
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet("stat/{accountId}")]
        public async Task<AccountStatContract> GetStat(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentNullException(nameof(accountId), "Account must be set.");
            }

            var stat = await _accountManagementService.GetStat(accountId);
            
            return _convertService.Convert<AccountStat, AccountStatContract>(stat);
        }

        /// <summary>
        /// Creates default accounts for client by trading condition id.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/default-accounts")]
        public Task<List<AccountContract>> CreateDefaultAccounts([NotNull] string clientId, 
            [NotNull] CreateDefaultAccountsRequest request)
        {
            return Convert(_accountManagementService.CreateDefaultAccountsAsync(
                clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)), 
                request.TradingConditionId.RequiredNotNullOrWhiteSpace(nameof(request.TradingConditionId))));
        }

        /// <summary>
        /// Create accounts with requested base asset for all users 
        /// that already have accounts with requested trading condition
        /// </summary>
        [HttpPost]
        [Route("new-base-asset")]
        public Task<List<AccountContract>> CreateAccountsForNewBaseAsset([NotNull] CreateAccountsForBaseAssetRequest request)
        {
            var account = _accountManagementService.CreateAccountsForNewBaseAssetAsync(
                request.TradingConditionId.RequiredNotNullOrWhiteSpace(nameof(request.TradingConditionId)),
                request.BaseAssetId.RequiredNotNullOrWhiteSpace(nameof(request.BaseAssetId)));
            return Convert(account);
        }

        private async Task<List<AccountContract>> Convert(Task<IReadOnlyList<IAccount>> accounts)
        {
            return (await accounts).Select(Convert).OrderBy(a => a.ClientId).ThenBy(a => a.BaseAssetId)
                .ThenBy(a => a.Id).ToList();
        }

        private async Task<AccountContract> Convert(Task<IAccount> accountTask)
        {
            return Convert(await accountTask);
        }

        private AccountContract Convert(IAccount account)
        {
            return _convertService.Convert<IAccount, AccountContract>(account);
        }

        private Account Convert(AccountContract account)
        {
            return _convertService.Convert<AccountContract, Account>(account);
        }

        private async Task ValidateClientAndAccountId(string clientId, string accountId)
        {
            var account = await _accountManagementService.GetByClientAndIdAsync(clientId, accountId);

            if (account == null)
            {
                throw new ArgumentException($"Account {accountId} of client {clientId} does not exist");
            }
        }
    }
}