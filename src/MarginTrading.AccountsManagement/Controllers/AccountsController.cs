using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;
using Refit;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/accounts")]
    public class AccountsController : Controller, IAccountsApi
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IAccuracyService _accuracyService;
        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly ISendBalanceCommandsService _sendBalanceCommandsService;

        public AccountsController(
            IAccountManagementService accountManagementService,
            IAccuracyService accuracyService,
            IConvertService convertService,
            ISystemClock systemClock,
            ISendBalanceCommandsService sendBalanceCommandsService)
        {
            _accountManagementService = accountManagementService;
            _accuracyService = accuracyService;
            _convertService = convertService;
            _systemClock = systemClock;
            _sendBalanceCommandsService = sendBalanceCommandsService;
        }

        #region CRUD
        
        /// <summary>
        /// Gets all accounts
        /// </summary>
        [HttpGet]
        [Route("")]
        public Task<List<AccountContract>> List([FromQuery] string search = null, bool showDeleted = false)
        {
            return Convert(_accountManagementService.ListAsync(search, showDeleted));
        }

        /// <summary>
        /// Gets all accounts, optionally paginated. Both skip and take must be set or unset.
        /// </summary>
        [HttpGet]
        [Route("by-pages")]
        public Task<PaginatedResponseContract<AccountContract>> ListByPages([FromQuery] string search = null,
            [FromQuery] int? skip = null, [FromQuery] int? take = null, bool showDeleted = false)
        {
            if ((skip.HasValue && !take.HasValue) || (!skip.HasValue && take.HasValue))
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Both skip and take must be set or unset");
            }

            if (take.HasValue && (take <= 0 || skip < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(skip), "Skip must be >= 0, take must be > 0");
            }
            
            return Convert(_accountManagementService.ListByPagesAsync(search, showDeleted, skip: skip, take: take));
        }

        /// <summary>
        /// Gets all accounts by <paramref name="clientId"/>
        /// </summary>
        [HttpGet]
        [Route("{clientId}")]
        public Task<List<AccountContract>> GetByClient([NotNull] string clientId, bool showDeleted = false)
        {
            return Convert(_accountManagementService.GetByClientAsync(
                clientId.RequiredNotNullOrWhiteSpace(nameof(clientId)), showDeleted));
        }

        /// <summary>
        /// Gets account by clientId and accountId
        /// </summary>
        [HttpGet]
        [Obsolete("Use GetById.")]
        [Route("{clientId}/{accountId}")]
        public Task<AccountContract> GetByClientAndId([NotNull] string clientId, [NotNull] string accountId)
        {
            return GetById(accountId);
        }

        /// <summary>
        /// Gets account by accountId
        /// </summary>
        [HttpGet]
        [Route("by-id/{accountId}")]
        public Task<AccountContract> GetById([NotNull] string accountId)
        {
            return Convert(_accountManagementService.GetByIdAsync(accountId.RequiredNotNullOrWhiteSpace(nameof(accountId))));
        }

        /// <summary>
        /// Creates an account
        /// </summary>
        [HttpPost]
        [Route("{clientId}")]
        [Obsolete("Use a single-parameter Create.")]
        public Task<AccountContract> Create([NotNull] string clientId, 
            [FromBody][NotNull] CreateAccountRequestObsolete request)
        {
            return Create(new CreateAccountRequest
            {
                ClientId = clientId,
                AccountId = request.AccountId,
                TradingConditionId = request.TradingConditionId,
                BaseAssetId = request.BaseAssetId,
            });
        }
        
        /// <summary>
        /// Creates an account
        /// </summary>
        [HttpPost]
        [Route("")]
        public Task<AccountContract> Create([FromBody][NotNull] CreateAccountRequest request)
        {
            return Convert(
                _accountManagementService.CreateAsync(
                    request.ClientId.RequiredNotNullOrWhiteSpace(nameof(request.ClientId)),
                    request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                    request.TradingConditionId,
                    request.BaseAssetId.RequiredNotNullOrWhiteSpace(nameof(request.BaseAssetId))));
        }

        /// <summary>
        /// Changes an account.
        /// </summary>
        [HttpPatch]
        [Route("{clientId}/{accountId}")]
        [Obsolete("Use a two-parameter Change.")]
        public async Task<AccountContract> Change([NotNull] string clientId, [NotNull] string accountId,
            [FromBody] [NotNull] ChangeAccountRequest request)
        {
            return await Change(accountId, request);
        }

        /// <summary>
        /// Changes an account.
        /// </summary>
        [HttpPatch]
        [Route("{accountId}")]
        public async Task<AccountContract> Change([NotNull] string accountId,
            [FromBody] [NotNull] ChangeAccountRequest request)
        {
            if (request.IsDisabled == null &&
                request.IsWithdrawalDisabled == null &&
                string.IsNullOrEmpty(request.TradingConditionId))
            {
                return await GetById(accountId);
            }

            var result = await _accountManagementService.UpdateAccountAsync(accountId,
                request.TradingConditionId,
                request.IsDisabled,
                request.IsWithdrawalDisabled);

            return Convert(result);
        }

        /// <summary>
        /// Delete an account.
        /// </summary>
        [HttpDelete("{accountId}")]
        public async Task<AccountContract> Delete(string accountId)
        {
            accountId.RequiredNotNullOrEmpty(nameof(accountId), $"{nameof(accountId)} must be set.");

            var result = await _accountManagementService.Delete(accountId);

            return Convert(result);
        }

        #endregion CRUD

        #region Deposit/Withdraw/ChargeManually
        
        /// <summary>
        /// Starts the operation of manually charging the client's account.
        /// Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance")]
        [Obsolete("Use a two-parameter BeginChargeManually.")]
        public async Task<string> BeginChargeManually([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeManuallyRequest request)
        {
            return await BeginChargeManually(accountId, request);
        }

        /// <summary>
        /// Starts the operation of manually charging the client's account.
        /// Amount is absolute, i.e. negative value goes for charging.
        /// </summary>
        [HttpPost]
        [Route("{accountId}/balance")]
        public async Task<string> BeginChargeManually([NotNull] string accountId,
            [FromBody][NotNull] AccountChargeManuallyRequest request)
        {
            var account = await _accountManagementService.EnsureAccountExistsAsync(accountId);
            _accountManagementService.EnsureAccountNotDeleted(account);

            var amount = await _accuracyService.ToAccountAccuracy(
                request.AmountDelta.RequiredNotEqualsTo(default, nameof(request.AmountDelta)),
                account.BaseAssetId, nameof(BeginChargeManually));

            return await _sendBalanceCommandsService.ChargeManuallyAsync(
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: amount,
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason,
                source: "Api",
                auditLog: request.AdditionalInfo,
                type: request.ReasonType.ToType<AccountBalanceChangeReasonType>(),
                eventSourceId: request.EventSourceId, 
                assetPairId: request.AssetPairId, 
                tradingDate: request.TradingDay ?? _systemClock.UtcNow.DateTime);
        }

        /// <summary>
        /// Starts the operation of depositing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/deposit")]
        [Obsolete("Use a two-parameter BeginDeposit.")]
        public async Task<string> BeginDeposit([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            return await BeginDeposit(accountId, request);
        }

        /// <summary>
        /// Starts the operation of depositing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{accountId}/balance/deposit")]
        public async Task<string> BeginDeposit([NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            var account = await _accountManagementService.EnsureAccountExistsAsync(accountId);
            _accountManagementService.EnsureAccountNotDeleted(account);

            var amount = await _accuracyService.ToAccountAccuracy(
                request.AmountDelta.RequiredGreaterThan(default, nameof(request.AmountDelta)),
                account.BaseAssetId, nameof(BeginDeposit));
            
            return await _sendBalanceCommandsService.DepositAsync(
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: amount,
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason,
                auditLog: request.AdditionalInfo);
        }

        /// <summary>
        /// Starts the operation of withdrawing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{clientId}/{accountId}/balance/withdraw")]
        [Obsolete("Use a two-parameter BeginWithdraw.")]
        public async Task<string> BeginWithdraw([NotNull] string clientId, [NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            return await BeginWithdraw(accountId, request);
        }

        /// <summary>
        /// Starts the operation of withdrawing funds to the client's account. Amount should be positive.
        /// </summary>
        [HttpPost]
        [Route("{accountId}/balance/withdraw")]
        public async Task<string> BeginWithdraw([NotNull] string accountId,
            [FromBody][NotNull] AccountChargeRequest request)
        {
            var account = await _accountManagementService.EnsureAccountExistsAsync(accountId);
            _accountManagementService.EnsureAccountNotDeleted(account);

            var amount = await _accuracyService.ToAccountAccuracy(
                request.AmountDelta.RequiredGreaterThan(default, nameof(request.AmountDelta)),
                account.BaseAssetId, nameof(BeginWithdraw));
            
            return await _sendBalanceCommandsService.WithdrawAsync(
                accountId: accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)),
                amountDelta: amount,
                operationId: request.OperationId.RequiredNotNullOrWhiteSpace(nameof(request.OperationId)),
                reason: request.Reason,
                auditLog: request.AdditionalInfo);
        }
        
        #endregion Deposit/Withdraw/ChargeManually

        #region System actions
        
        /// <summary>
        /// Reset account balance to default value (from settings)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("{accountId}/reset")]
        public async Task Reset([NotNull] string accountId)
        {
            await _accountManagementService.ResetAccountAsync(accountId.RequiredNotNullOrWhiteSpace(nameof(accountId)));
        }

        /// <summary>
        /// Creates default accounts for client by trading condition id.
        /// </summary>
        [HttpPost]
        [Route("default-accounts")]
        public Task<List<AccountContract>> CreateDefaultAccounts([NotNull] CreateDefaultAccountsRequest request)
        {
            return Convert(_accountManagementService.CreateDefaultAccountsAsync(
                request.ClientId.RequiredNotNullOrWhiteSpace(nameof(request.ClientId)), 
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

        

        #endregion System actions

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

            return stat != null ? _convertService.Convert<AccountStat, AccountStatContract>(stat) : null;
        }

        private async Task<PaginatedResponseContract<AccountContract>> Convert(Task<PaginatedResponse<IAccount>> accounts)
        {
            var data = await accounts;
            return new PaginatedResponseContract<AccountContract>(
                contents: data.Contents.Select(Convert).ToList(),
                start: data.Start,
                size: data.Size,
                totalSize: data.TotalSize
            );
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
    }
}