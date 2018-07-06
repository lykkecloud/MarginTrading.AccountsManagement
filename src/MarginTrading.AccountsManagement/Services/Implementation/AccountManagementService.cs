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
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.CodeAnalysis.Operations;

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

        public Task<IReadOnlyList<IAccount>> GetByClientAsync(string clientId)
        {
            return _accountsRepository.GetAllAsync(clientId: clientId);
        }

        public Task<IAccount> GetByClientAndIdAsync(string clientId, string accountId)
        {
            return _accountsRepository.GetAsync(clientId, accountId);
        }

        #endregion


        #region Modify

        public async Task<IAccount> SetTradingConditionAsync(string clientId, string accountId,
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
            _eventSender.SendAccountChangedEvent(nameof(SetTradingConditionAsync), result, 
                AccountChangedEventTypeContract.Updated);
            return result;
        }

        public async Task<IAccount> SetDisabledAsync(string clientId, string accountId, bool isDisabled)
        {
            var account = await _accountsRepository.ChangeIsDisabledAsync(clientId, accountId, isDisabled);
            _eventSender.SendAccountChangedEvent(nameof(SetTradingConditionAsync), account, 
                AccountChangedEventTypeContract.Updated);
            return account;
        }

        public async Task<IAccount> ResetAccountAsync(string clientId, string accountId)
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

        private async Task<IAccount> CreateAccount(string clientId, string baseAssetId, string tradingConditionId,
            string legalEntityId, string accountId = null)
        {
            var id = string.IsNullOrEmpty(accountId)
                ? $"{_settings.Behavior?.AccountIdPrefix}{Guid.NewGuid():N}"
                : accountId;

            IAccount account = new Account(id, clientId, tradingConditionId, baseAssetId, 0, 0, legalEntityId, false, 
                DateTime.UtcNow);

            await _accountsRepository.AddAsync(account);
            account = await _accountsRepository.GetAsync(account.ClientId, accountId);

            _eventSender.SendAccountChangedEvent(nameof(CreateAccount), account, 
                AccountChangedEventTypeContract.Created);

            if (_settings.Behavior?.DefaultBalance != null)
            {
                account = await UpdateBalanceAsync(Guid.NewGuid().ToString(), account.ClientId, account.Id, _settings.Behavior.DefaultBalance);
            }

            return account;
        }

        private async Task<IAccount> UpdateBalanceAsync(string operationId, string clientId, string accountId,
            decimal amountDelta, bool changeTransferLimit = false)
        {
            // todo: move to workflow command handler
            var account = await _accountsRepository.UpdateBalanceAsync(operationId, clientId, accountId, amountDelta, changeTransferLimit);
            _eventSender.SendAccountChangedEvent(nameof(UpdateBalanceAsync), account, 
                AccountChangedEventTypeContract.BalanceUpdated);
            return account;
        }

        #endregion
    }
}