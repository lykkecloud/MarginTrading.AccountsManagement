using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    [UsedImplicitly]
    internal class AccountManagementService : IAccountManagementService
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ITradingConditionsService _tradingConditionsService;
        private readonly AccountManagementSettings _settings;
        private readonly IEventSender _eventSender;
        private readonly ILog _log;

        public AccountManagementService(IAccountsRepository accountsRepository,
            ITradingConditionsService tradingConditionsService, AccountManagementSettings settings,
            IEventSender eventSender, ILog log)
        {
            _accountsRepository = accountsRepository;
            _tradingConditionsService = tradingConditionsService;
            _settings = settings;
            _eventSender = eventSender;
            _log = log;
        }


        #region Create 

        public async Task<Account> CreateAsync(string clientId, string accountId, string tradingConditionId,
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

            if (clientAccounts.Any(a => a.BaseAssetId == baseAssetId && a.TradingConditionId == tradingConditionId))
            {
                throw new NotSupportedException(
                    $"Client [{clientId}] already has account with base asset [{baseAssetId}] and trading condition [{tradingConditionId}]");
            }

            #endregion

            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            return await CreateAccount(clientId, baseAssetId, tradingConditionId, legalEntity, accountId);
        }

        public async Task<List<Account>> CreateDefaultAccountsAsync(string clientId, string tradingConditionId)
        {
            var existingAccounts = (await _accountsRepository.GetAllAsync(clientId)).ToList();

            if (existingAccounts.Any())
            {
                return existingAccounts;
            }

            if (string.IsNullOrEmpty(tradingConditionId))
                throw new ArgumentNullException(nameof(tradingConditionId));

            var baseAssets = await _tradingConditionsService.GetBaseAccountAssetsAsync(tradingConditionId);
            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            var newAccounts = new List<Account>();

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

            return newAccounts;
        }

        public async Task<List<Account>> CreateAccountsForNewBaseAssetAsync(string tradingConditionId,
            string baseAssetId)
        {
            var result = new List<Account>();

            var clientAccountGroups = (await _accountsRepository.GetAllAsync()).GroupBy(a => a.ClientId).Where(g =>
                g.Any(a => a.TradingConditionId == tradingConditionId) && g.All(a => a.BaseAssetId != baseAssetId));
            var legalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            foreach (var group in clientAccountGroups)
            {
                try
                {
                    var account = await CreateAccount(group.Key, baseAssetId, tradingConditionId, legalEntity);
                    result.Add(account);
                }
                catch (Exception e)
                {
                    _log.WriteError(nameof(AccountManagementService),
                        $"Create accounts by account group : clientId={group.Key}, tradingConditionId={tradingConditionId}, baseAssetId={baseAssetId}",
                        e);
                }
            }

            return result;
        }

        #endregion


        #region Get

        public Task<List<Account>> ListAsync()
        {
            return _accountsRepository.GetAllAsync();
        }

        public Task<List<Account>> GetByClientAsync(string clientId)
        {
            return _accountsRepository.GetAllAsync(clientId);
        }

        public Task<Account> GetByClientAndIdAsync(string clientId, string accountId)
        {
            return _accountsRepository.GetAsync(clientId, accountId);
        }

        #endregion


        #region Modify

        public async Task<Account> SetTradingConditionAsync(string clientId, string accountId,
            string tradingConditionId)
        {
            if (!await _tradingConditionsService.IsTradingConditionExistsAsync(tradingConditionId))
            {
                throw new ArgumentOutOfRangeException(nameof(tradingConditionId),
                    $"No trading condition {tradingConditionId} exists");
            }

            var account = await _accountsRepository.GetAsync(clientId, accountId);

            if (account == null)
                throw new ArgumentOutOfRangeException(
                    $"Account for client [{clientId}] with id [{accountId}] does not exist");

            var currentLegalEntity = account.LegalEntity;
            var newLegalEntity = await _tradingConditionsService.GetLegalEntityAsync(tradingConditionId);

            if (currentLegalEntity != newLegalEntity)
            {
                throw new NotSupportedException(
                    $"Account for client [{clientId}] with id [{accountId}] has LegalEntity " +
                    $"[{account.LegalEntity}], but trading condition wiht id [{tradingConditionId}] has " +
                    $"LegalEntity [{newLegalEntity}]");
            }

            var result =
                await _accountsRepository.UpdateTradingConditionIdAsync(clientId, accountId, tradingConditionId);
            _eventSender.SendAccountChangedEvent(result, AccountChangedEventTypeContract.Updated);
            return result;
        }

        public async Task<Account> SetDisabledAsync(string clientId, string accountId, bool isDisabled)
        {
            var account = await _accountsRepository.ChangeIsDisabledAsync(clientId, accountId, isDisabled);
            _eventSender.SendAccountChangedEvent(account, AccountChangedEventTypeContract.Updated);
            return account;
        }

        public async Task<Account> ResetAccountAsync(string clientId, string accountId)
        {
            if (_settings.Behavior?.BalanceResetIsEnabled != true)
            {
                throw new NotSupportedException("Account reset is not supported");
            }

            var account = await _accountsRepository.GetAsync(clientId, accountId);

            if (account == null)
                throw new ArgumentOutOfRangeException(
                    $"Account for client [{clientId}] with id [{accountId}] does not exist");

            return await UpdateBalanceAsync(Guid.NewGuid().ToString(), clientId, accountId, _settings.Behavior.DefaultBalance - account.Balance);
        }

        #endregion


        #region Helpers

        private async Task<Account> CreateAccount(string clientId, string baseAssetId, string tradingConditionId,
            string legalEntityId, string accountId = null)
        {
            var id = string.IsNullOrEmpty(accountId)
                ? $"{_settings.Behavior?.AccountIdPrefix}{Guid.NewGuid():N}"
                : accountId;

            var account = new Account(id, clientId, tradingConditionId, baseAssetId, 0, 0, legalEntityId, false, default);

            await _accountsRepository.AddAsync(account);
            account = await _accountsRepository.GetAsync(account.ClientId, accountId);

            _eventSender.SendAccountChangedEvent(account, AccountChangedEventTypeContract.Created);

            if (_settings.Behavior?.DefaultBalance != null)
            {
                account = await UpdateBalanceAsync(Guid.NewGuid().ToString(), account.ClientId, account.Id, _settings.Behavior.DefaultBalance);
            }

            return account;
        }

        private async Task<Account> UpdateBalanceAsync(string operationId, string clientId, string accountId, decimal amountDelta,
            bool changeTransferLimit = false)
        {
            // todo: move to workflow command handler
            var account = await _accountsRepository.UpdateBalanceAsync(operationId, clientId, accountId, amountDelta, changeTransferLimit);
            _eventSender.SendAccountChangedEvent(account, AccountChangedEventTypeContract.BalanceUpdated);
            return account;
        }

        #endregion
    }
}