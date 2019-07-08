// Copyright (c) 2019 Lykke Corp.

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
        public const string OperationName = "DeleteAccounts";
        private readonly IChaosKitty _chaosKitty;
        private readonly ISystemClock _systemClock;
        private readonly CqrsContextNamesSettings _contextNames;

        public DeleteAccountsSaga(
            IOperationExecutionInfoRepository executionInfoRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            CqrsContextNamesSettings contextNames)
        {
            _executionInfoRepository = executionInfoRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _contextNames = contextNames;
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
                executionInfo.Data.AddFailedIfNotExist(e.FailedAccountIds);

                sender.SendCommand(new BlockAccountsForDeletionCommand
                    {
                        OperationId = e.OperationId,
                        Timestamp = _systemClock.UtcNow.UtcDateTime,
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
                        Timestamp = _systemClock.UtcNow.UtcDateTime,
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
                        Timestamp = _systemClock.UtcNow.UtcDateTime,
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
                        Timestamp = _systemClock.UtcNow.UtcDateTime,
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
        /// Account deletion is finished
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AccountsDeletionFinishedEvent e, ICommandSender sender)
        {
            //nothing to do
            return Task.CompletedTask;
        }
    }
}