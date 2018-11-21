using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.Backend.Contracts;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    [UsedImplicitly]
    internal class AccountManagementService : IAccountManagementService
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly ITradingConditionsService _tradingConditionsService;
        private readonly ISendBalanceCommandsService _sendBalanceCommandsService;
        private readonly IOrdersApi _ordersApi;
        private readonly IPositionsApi _positionsApi;
        private readonly AccountManagementSettings _settings;
        private readonly IEventSender _eventSender;
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;

        public AccountManagementService(IAccountsRepository accountsRepository,
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            ITradingConditionsService tradingConditionsService,
            ISendBalanceCommandsService sendBalanceCommandsService,
            IOrdersApi ordersApi,
            IPositionsApi positionsApi,
            AccountManagementSettings settings,
            IEventSender eventSender, 
            ILog log,
            ISystemClock systemClock)
        {
            _accountsRepository = accountsRepository;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _tradingConditionsService = tradingConditionsService;
            _sendBalanceCommandsService = sendBalanceCommandsService;
            _ordersApi = ordersApi;
            _positionsApi = positionsApi;
            _settings = settings;
            _eventSender = eventSender;
            _log = log;
            _systemClock = systemClock;
        }


        #region Create 

        public async Task<IAccount> CreateAsync(string clientId, string accountId, string tradingConditionId,
            string baseAssetId)
        {
            #region Validations

            if (string.IsNullOrEmpty(tradingConditionId))
            {
                tradingConditionId = await _tradingConditionsService.GetDefaultTradingConditionIdAsync()
                    .RequiredNotNull("default trading condition");
            }

            var baseAssetExists = _tradingConditionsService.IsBaseAssetExistsAsync(tradingConditionId, baseAssetId);

            if (!await baseAssetExists)
            {
                throw new ArgumentOutOfRangeException(nameof(tradingConditionId),
                    $"Base asset [{baseAssetId}] is not configured for trading condition [{tradingConditionId}]");
            }

            var clientAccounts = await GetByClientAsync(clientId);

            if (!string.IsNullOrEmpty(accountId) && clientAccounts.Any(a => a.Id == accountId))
            {
                throw new NotSupportedException($"Client [{clientId}] already has account with ID [{accountId}]");
            }

            #endregion

            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            var account = await CreateAccount(clientId, baseAssetId, tradingConditionId, legalEntity, accountId);

            _log.WriteInfo(nameof(AccountManagementService), nameof(CreateAsync),
                $"{baseAssetId} account {accountId} created for client {clientId} on trading condition {tradingConditionId}");
            
            return account;
        }

        public async Task<IReadOnlyList<IAccount>> CreateDefaultAccountsAsync(string clientId,
            string tradingConditionId)
        {
            var existingAccounts = (await _accountsRepository.GetAllAsync(clientId: clientId)).ToList();

            if (existingAccounts.Any())
            {
                return existingAccounts;
            }

            if (string.IsNullOrEmpty(tradingConditionId))
                throw new ArgumentNullException(nameof(tradingConditionId));

            var baseAssets = await _tradingConditionsService.GetBaseAccountAssetsAsync(tradingConditionId);
            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            var newAccounts = new List<IAccount>();

            foreach (var baseAsset in baseAssets)
            {
                try
                {
                    var account = await CreateAccount(clientId, baseAsset, tradingConditionId, legalEntity);
                    newAccounts.Add(account);
                }
                catch (Exception e)
                {
                    _log.WriteError(nameof(AccountManagementService),
                        $"Create default accounts: clientId={clientId}, tradingConditionId={tradingConditionId}", e);
                }
            }

            _log.WriteInfo(nameof(AccountManagementService), "CreateDefaultAccountsAsync",
                $"{string.Join(", ", newAccounts.Select(x => x.BaseAssetId))} accounts created for client {clientId}");

            return newAccounts;
        }

        public async Task<IReadOnlyList<IAccount>> CreateAccountsForNewBaseAssetAsync(string tradingConditionId,
            string baseAssetId)
        {
            var result = new List<IAccount>();

            var clientAccountGroups = (await _accountsRepository.GetAllAsync()).GroupBy(a => a.ClientId).Where(g =>
                g.Any(a => a.TradingConditionId == tradingConditionId) && g.All(a => a.BaseAssetId != baseAssetId));
            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            foreach (var group in clientAccountGroups)
            {
                try
                {
                    var account = await CreateAccount(@group.Key, baseAssetId, tradingConditionId, legalEntity);
                    result.Add(account);
                }
                catch (Exception e)
                {
                    _log.WriteError(nameof(AccountManagementService),
                        $"Create accounts by account group : clientId={group.Key}, tradingConditionId={tradingConditionId}, baseAssetId={baseAssetId}",
                        e);
                }
            }

            _log.WriteInfo(nameof(AccountManagementService), nameof(CreateAccountsForNewBaseAssetAsync),
                $"{result.Count} accounts created for the new base asset {baseAssetId} in trading condition {tradingConditionId}");

            return result;
        }

        #endregion


        #region Get

        public Task<IReadOnlyList<IAccount>> ListAsync(string search)
        {
            return _accountsRepository.GetAllAsync(search: search);
        }

        public Task<PaginatedResponse<IAccount>> ListByPagesAsync(string search = null, int? skip = null, int? take = null)
        {
            return _accountsRepository.GetByPagesAsync(search, skip, take);
        }

        public Task<IReadOnlyList<IAccount>> GetByClientAsync(string clientId)
        {
            return _accountsRepository.GetAllAsync(clientId: clientId);
        }

        public Task<IAccount> GetByIdAsync(string accountId)
        {
            return _accountsRepository.GetAsync(accountId);
        }

        public async Task<AccountStat> GetStat(string accountId)
        {
            var accountHistory = (await _accountBalanceChangesRepository.GetAsync(
                accountId: accountId,
                //TODO rethink the way trading day's start & end are selected 
                from: _systemClock.UtcNow.UtcDateTime.Date
            )).ToList();

            var sortedHistory = accountHistory.OrderByDescending(x => x.ChangeTimestamp).ToList();
            var firstEvent = sortedHistory.LastOrDefault();
            var account = await _accountsRepository.GetAsync(accountId);
            
            if (account == null)
                return null;
            
            return new AccountStat(
                accountId: accountId,
                created: _systemClock.UtcNow.UtcDateTime,
                realisedPnl: accountHistory.Where(x => x.ReasonType == AccountBalanceChangeReasonType.RealizedPnL)
                    .Sum(x => x.ChangeAmount),// todo recheck!
                depositAmount: accountHistory.Where(x => x.ReasonType == AccountBalanceChangeReasonType.Deposit)
                    .Sum(x => x.ChangeAmount),
                withdrawalAmount: accountHistory.Where(x => x.ReasonType == AccountBalanceChangeReasonType.Withdraw)
                    .Sum(x => x.ChangeAmount),
                commissionAmount: accountHistory.Where(x => x.ReasonType == AccountBalanceChangeReasonType.Commission)
                    .Sum(x => x.ChangeAmount),
                otherAmount: accountHistory.Where(x => !new []
                {
                    AccountBalanceChangeReasonType.RealizedPnL,
                    AccountBalanceChangeReasonType.Deposit,
                    AccountBalanceChangeReasonType.Withdraw,
                    AccountBalanceChangeReasonType.Commission,
                }.Contains(x.ReasonType)).Sum(x => x.ChangeAmount),
                accountBalance: account.Balance,
                prevEodAccountBalance: (firstEvent?.Balance - firstEvent?.ChangeAmount) ?? account.Balance
            );
        }

        #endregion


        #region Modify

        public async Task<IAccount> UpdateAccountAsync(string accountId,
            string tradingConditionId, bool? isDisabled, bool? isWithdrawalDisabled)
        {
            await ValidateTradingConditionAsync(accountId, tradingConditionId);

            await ValidateIfDisableIsAvailableAsync(accountId, isDisabled);

            var result = await _accountsRepository.UpdateAccountAsync(accountId, tradingConditionId, isDisabled,
                isWithdrawalDisabled);
            _eventSender.SendAccountChangedEvent(nameof(UpdateAccountAsync), result, 
                AccountChangedEventTypeContract.Updated, Guid.NewGuid().ToString("N"));
                
            return result;
        }

        public async Task<IAccount> ResetAccountAsync(string accountId)
        {
            if (_settings.Behavior?.BalanceResetIsEnabled != true)
            {
                throw new NotSupportedException("Account reset is not supported");
            }

            if (!(await _accountsRepository.GetAsync(accountId) is Account account))
            {
                throw new ArgumentOutOfRangeException($"Account with id [{accountId}] does not exist");
            }

            await UpdateBalanceAsync(Guid.NewGuid().ToString(), accountId, 
                _settings.Behavior.DefaultBalance - account.Balance, AccountBalanceChangeReasonType.Reset, 
                "Reset account Api");

            account.Balance = _settings.Behavior.DefaultBalance;
            return account;
        }

        #endregion


        #region Helpers

        private async Task<IAccount> CreateAccount(string clientId, string baseAssetId, string tradingConditionId,
            string legalEntityId, string accountId = null)
        {
            var id = string.IsNullOrEmpty(accountId)
                ? $"{_settings.Behavior?.AccountIdPrefix}{Guid.NewGuid():N}"
                : accountId;

            var account = new Account(id, clientId, tradingConditionId, baseAssetId, 0, 0, legalEntityId, false, 
                !(_settings.Behavior?.DefaultWithdrawalIsEnabled ?? true), DateTime.UtcNow);

            await _accountsRepository.AddAsync(account);
            account = (Account) await _accountsRepository.GetAsync(accountId);

            _eventSender.SendAccountChangedEvent(nameof(CreateAccount), account,
                AccountChangedEventTypeContract.Created, id);

            if (_settings.Behavior?.DefaultBalance != null && _settings.Behavior.DefaultBalance != default)
            {
                await UpdateBalanceAsync(Guid.NewGuid().ToString(), account.Id, _settings.Behavior.DefaultBalance, 
                    AccountBalanceChangeReasonType.Create, "Create account Api");
                account.Balance = _settings.Behavior.DefaultBalance;
            }

            return account;
        }

        private async Task UpdateBalanceAsync(string operationId, string accountId,
            decimal amountDelta, AccountBalanceChangeReasonType changeReasonType, string source, bool changeTransferLimit = false)
        {
            await _sendBalanceCommandsService.ChargeManuallyAsync(
                accountId: accountId,
                amountDelta: amountDelta,
                operationId: operationId,
                reason: changeReasonType.ToString(),
                source: source,
                auditLog: null,
                type: changeReasonType,
                eventSourceId: operationId,
                assetPairId: null,
                tradingDate: _systemClock.UtcNow.UtcDateTime);
        }
        
        private async Task ValidateTradingConditionAsync(string accountId,
            string tradingConditionId)
        {
            if (string.IsNullOrEmpty(tradingConditionId))
                return;
            
            if (!await _tradingConditionsService.IsTradingConditionExistsAsync(tradingConditionId))
            {
                throw new ArgumentOutOfRangeException(nameof(tradingConditionId),
                    $"No trading condition {tradingConditionId} exists");
            }

            var account = await _accountsRepository.GetAsync(accountId);

            if (account == null)
                throw new ArgumentOutOfRangeException(
                    $"Account with id [{accountId}] does not exist");

            var currentLegalEntity = account.LegalEntity;
            var newLegalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            if (currentLegalEntity != newLegalEntity)
            {
                throw new NotSupportedException(
                    $"Account with id [{accountId}] has LegalEntity " +
                    $"[{account.LegalEntity}], but trading condition with id [{tradingConditionId}] has " +
                    $"LegalEntity [{newLegalEntity}]");
            }
        }

        private async Task ValidateIfDisableIsAvailableAsync(string accountId, bool? isDisabled)
        {
            if (isDisabled == null || isDisabled == false)
                return;
            
            var ordersTask = _ordersApi.ListAsync(accountId);
            var positionsTask = _positionsApi.ListAsync(accountId);
            var orders = await ordersTask;
            var positions = await positionsTask;

            if (orders.Any() || positions.Any())
            {
                throw new ValidationException($"Account disabling is only available when there are no orders ({orders.Count}) and positions ({positions.Count}).");
            }
        }

        #endregion
    }
}