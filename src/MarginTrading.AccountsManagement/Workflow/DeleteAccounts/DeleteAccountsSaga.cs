using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands;
using MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Events;
using Microsoft.Extensions.Internal;
using MoreLinq;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts
{
    internal class DeleteAccountsSaga
    {
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        public const string OperationName = "DeleteAccounts";
        private readonly IChaosKitty _chaosKitty;
        private readonly ISystemClock _systemClock;
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly AccountManagementSettings _settings;

        public DeleteAccountsSaga(
            IOperationExecutionInfoRepository executionInfoRepository,
            IAccountsRepository accountsRepository,
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            CqrsContextNamesSettings contextNames,
            AccountManagementSettings settings)
        {
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _contextNames = contextNames;
            _settings = settings;
        }

        /// <summary>
        /// The operation has started => ask MT Core to block trades and withdrawals on accounts
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DeleteAccountsStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(DeleteAccountsState.Initiated, DeleteAccountsState.Started))
            {
                executionInfo.Data.AddFailedIfNotExist(
                    await ValidateAccountsAsync(executionInfo.Data.AccountIds, _accountsRepository, 
                        _accountBalanceChangesRepository, _systemClock.UtcNow.UtcDateTime));

                sender.SendCommand(new BlockAccountsForDeletionCommand
                    {
                        OperationId = e.OperationId,
                        AccountIds = executionInfo.Data.GetAccountsToDelete(),
                    },
                    _contextNames.TradingEngine);

                _chaosKitty.Meow(
                    $"{nameof(DeleteAccountsStartedInternalEvent)}: " +
                    "Save_OperationExecutionInfo: " +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
        
        /// <summary>
        /// Account are blocked on side of MT Core => validate & mark as deleted on side of Accounts Management
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountsBlockedForDeletionEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(DeleteAccountsState.Started, DeleteAccountsState.MtCoreAccountsBlocked))
            {
                executionInfo.Data.AddFailedIfNotExist(e.FailedAccountIds);
                
                sender.SendCommand(new MarkAccountsAsDeletedInternalCommand
                    {
                        OperationId = e.OperationId,
                    },
                    _contextNames.AccountsManagement);

                _chaosKitty.Meow(
                    $"{nameof(AccountsBlockedForDeletionEvent)}: " +
                    "Save_OperationExecutionInfo: " +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
        
        /// <summary>
        /// Accounts are marked as deleted on side of Accounts Management =>
        ///     remove successfully deleted accounts from cache on side of MT Core
        ///     & unblock trades and withdrawals for failed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountsMarkedAsDeletedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(DeleteAccountsState.MtCoreAccountsBlocked, DeleteAccountsState.AccountsMarkedAsDeleted))
            {
                executionInfo.Data.AddFailedIfNotExist(e.ValidationFailedAccountIds);
                
                sender.SendCommand(new MtCoreFinishAccountsDeletionCommand
                    {
                        OperationId = e.OperationId,
                        AccountIds = executionInfo.Data.GetAccountsToDelete(),
                        FailedAccountIds = executionInfo.Data.FailedAccountIds.Keys.ToList(),
                    },
                    _contextNames.TradingEngine);

                _chaosKitty.Meow(
                    $"{nameof(AccountsMarkedAsDeletedEvent)}: " +
                    "Save_OperationExecutionInfo: " +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
        
        /// <summary>
        /// Account deletion is finished on side of Trading Engine => send command to generate final event
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(MtCoreDeleteAccountsFinishedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(DeleteAccountsState.AccountsMarkedAsDeleted, DeleteAccountsState.Finished))
            {
                sender.SendCommand(new FinishAccountsDeletionInternalCommand
                    {
                        OperationId = e.OperationId,
                    },
                    _contextNames.AccountsManagement);

                _chaosKitty.Meow(
                    $"{nameof(MtCoreDeleteAccountsFinishedEvent)}: " +
                    "Save_OperationExecutionInfo: " +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
        
        /// <summary>
        /// Account deletion is finished => generate final event
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountsDeletionFinishedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.State != DeleteAccountsState.Finished)
            {
                throw new Exception($"{nameof(AccountsDeletionFinishedEvent)} have state {executionInfo.Data.State.ToString()}, but [{DeleteAccountsState.Finished}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            await _executionInfoRepository.DeleteAsync(executionInfo);
        }

        /// <summary>
        /// Validate accounts for deletion.
        /// </summary>
        /// <param name="accountIdsToValidate">Accounts to validate.</param>
        /// <param name="accountsRepository"></param>
        /// <param name="accountBalanceChangesRepository"></param>
        /// <param name="now"></param>
        /// <returns>Dictionary of failed accountIds with fail reason.</returns>
        public static async Task<Dictionary<string, string>> ValidateAccountsAsync(
            IEnumerable<string> accountIdsToValidate,
            IAccountsRepository accountsRepository,
            IAccountBalanceChangesRepository accountBalanceChangesRepository, DateTime now)
        {
            var failedAccounts = new Dictionary<string, string>();
            
            foreach (var accountId in accountIdsToValidate)
            {
                var account = await accountsRepository.GetAsync(accountId);

                if (account == null)
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] does not exist");
                    continue;
                }

                if (account.IsDeleted)
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] is deleted. No operations are permitted.");
                    continue;
                }

                if (account.Balance != 0)
                {
                    failedAccounts.Add(accountId, 
                        $"Account [{accountId}] balance is non-zero, so it cannot be deleted.");
                    continue;
                }

                var todayTransactions = await accountBalanceChangesRepository.GetAsync(accountId, now.Date);
                if (todayTransactions.Any())
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] had {todayTransactions.Count} transactions today. Please try to delete an account tomorrow.");
                    continue;
                }
            }

            return failedAccounts;
        }
    }
}