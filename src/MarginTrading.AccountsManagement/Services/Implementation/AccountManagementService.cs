using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Extensions;
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

        public Task<IReadOnlyList<IAccount>> ListAsync(string search, bool showDeleted = false)
        {
            return _accountsRepository.GetAllAsync(search: search, showDeleted: showDeleted);
        }

        public Task<PaginatedResponse<IAccount>> ListByPagesAsync(string search, bool showDeleted = false,
            int? skip = null, int? take = null)
        {
            return _accountsRepository.GetByPagesAsync(search, showDeleted, skip: skip, take: take);
        }

        public Task<IReadOnlyList<IAccount>> GetByClientAsync(string clientId, bool showDeleted = false)
        {
            return _accountsRepository.GetAllAsync(clientId: clientId, showDeleted: showDeleted);
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
                @from: _systemClock.UtcNow.UtcDateTime.Date)).ToList();

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

        public async Task<IAccount> EnsureAccountExistsAsync(string accountId)
        {
            var account = await GetByIdAsync(accountId);

            if (account == null)
            {
                throw new ArgumentException($"Account [{accountId}] does not exist");
            }

            return account;
        }

        #endregion


        #region Modify

        public async Task<IAccount> UpdateAccountAsync(string accountId,
            string tradingConditionId, bool? isDisabled, bool? isWithdrawalDisabled)
        {
            await ValidateTradingConditionAsync(accountId, tradingConditionId);

            var account = await EnsureAccountExistsAsync(accountId);
            EnsureAccountNotDeleted(account);

            var result =
                await _accountsRepository.UpdateAccountAsync(
                    accountId,
                    tradingConditionId,
                    isDisabled,
                    isWithdrawalDisabled);
            
            _eventSender.SendAccountChangedEvent(
                nameof(UpdateAccountAsync),
                result,
                AccountChangedEventTypeContract.Updated,
                Guid.NewGuid().ToString("N"),
                previousSnapshot: account);
                
            return result;
        }

        public async Task<IAccount> Delete(string accountId)
        {
            var account = await EnsureAccountExistsAsync(accountId);

            EnsureAccountNotDeleted(account);

            if (account.Balance != 0)
            {
                throw new ValidationException($"Account [{accountId}] balance is non-zero, so it cannot be deleted.");
            }

            var ordersTask = _ordersApi.ListAsync(accountId);
            var positionsTask = _positionsApi.ListAsync(accountId);
            var orders = await ordersTask;
            var positions = await positionsTask;
            if (orders.Any() || positions.Any())
            {
                throw new ValidationException($"Account deletion is only available when there are no orders ({orders.Count}) and positions ({positions.Count}).");
            }

            var result = await _accountsRepository.DeleteAsync(accountId);
            
            _eventSender.SendAccountChangedEvent(
                nameof(Delete),
                result,
                AccountChangedEventTypeContract.Deleted,
                Guid.NewGuid().ToString("N"),
                previousSnapshot: account);

            return result;
        }

        public async Task ResetAccountAsync(string accountId)
        {
            if (_settings.Behavior?.BalanceResetIsEnabled != true)
            {
                throw new NotSupportedException("Account reset is not supported");
            }

            var account = await EnsureAccountExistsAsync(accountId);
            EnsureAccountNotDeleted(account);

            await UpdateBalanceAsync(Guid.NewGuid().ToString(), accountId, 
                _settings.Behavior.DefaultBalance - account.Balance, AccountBalanceChangeReasonType.Reset, 
                "Reset account Api");
        }

        public async Task<string> StartGiveTemporaryCapital(string eventSourceId, string accountId, decimal amount,
            string reason, string comment, string additionalInfo)
        {
            return await _sendBalanceCommandsService.GiveTemporaryCapital(
                eventSourceId, 
                accountId,
                amount, 
                reason, 
                comment, 
                additionalInfo);
        }

        public async Task<string> StartRevokeTemporaryCapital(string eventSourceId, string accountId,
            string revokeEventSourceId, string comment, string additionalInfo)
        {
            return await _sendBalanceCommandsService.RevokeTemporaryCapital(
                eventSourceId, 
                accountId,
                revokeEventSourceId, 
                comment, 
                additionalInfo);
        }

        #endregion


        #region Helpers

        public void EnsureAccountNotDeleted(IAccount account)
        {
            if (account.IsDeleted)
            {
                throw new ValidationException($"Account [{account.Id}] is deleted. No operations are permitted.");
            }
        }

        private async Task<IAccount> CreateAccount(string clientId, string baseAssetId, string tradingConditionId,
            string legalEntityId, string accountId = null)
        {
            var id = string.IsNullOrEmpty(accountId)
                ? $"{_settings.Behavior?.AccountIdPrefix}{Guid.NewGuid():N}"
                : accountId;

            IAccount account = new Account(
                id, 
                clientId, 
                tradingConditionId, 
                baseAssetId, 
                0, 
                0, 
                legalEntityId, 
                false, 
                !(_settings.Behavior?.DefaultWithdrawalIsEnabled ?? true), 
                false,
                DateTime.UtcNow);

            await _accountsRepository.AddAsync(account);
            account = await _accountsRepository.GetAsync(accountId);

            _eventSender.SendAccountChangedEvent(nameof(CreateAccount), account,
                AccountChangedEventTypeContract.Created, id);

            //todo consider moving to CQRS projection
            if (_settings.Behavior?.DefaultBalance != null && _settings.Behavior.DefaultBalance != default)
            {
                await UpdateBalanceAsync(Guid.NewGuid().ToString(), account.Id, _settings.Behavior.DefaultBalance, 
                    AccountBalanceChangeReasonType.Create, "Create account Api");
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

        public static List<TemporaryCapital> UpdateTemporaryCapital(string accountId, List<TemporaryCapital> current,
            TemporaryCapital temporaryCapital, bool isAdd)
        {
            var result = current.ToList();
            
            if (isAdd)
            {
                if (result.Any(x => x.Id == temporaryCapital.Id))
                {
                    throw new ArgumentException(
                        $"Temporary capital record with id {temporaryCapital.Id} is already set on account {accountId}",
                        nameof(temporaryCapital.Id));
                }
                    
                if (temporaryCapital != null)
                {
                    result.Add(temporaryCapital);
                }
            }
            else
            {
                if (temporaryCapital != null)
                {
                    result.RemoveAll(x => x.Id == temporaryCapital.Id);
                }
                else
                {
                    result.Clear();
                }
            }

            return result;
        }

        #endregion
    }
}