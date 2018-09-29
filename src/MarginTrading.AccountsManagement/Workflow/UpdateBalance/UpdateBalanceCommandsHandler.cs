using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    internal class UpdateBalanceCommandsHandler
    {
        private readonly INegativeProtectionService _negativeProtectionService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ISystemClock _systemClock;
        private readonly IConvertService _convertService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;

        public UpdateBalanceCommandsHandler(IOperationExecutionInfoRepository executionInfoRepository,
            INegativeProtectionService negativeProtectionService,
            IAccountsRepository accountsRepository,
            IChaosKitty chaosKitty, 
            ISystemClock systemClock,
            IConvertService convertService)
        {
            _negativeProtectionService = negativeProtectionService;
            _accountsRepository = accountsRepository;
            _chaosKitty = chaosKitty;
            _systemClock = systemClock;
            _convertService = convertService;
            _executionInfoRepository = executionInfoRepository;
        }

        /// <summary>
        /// Handles internal command to change the balance
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(UpdateBalanceInternalCommand command,
            IEventPublisher publisher)
        {
            IAccount account = null;
            try
            {
                account = await _accountsRepository.UpdateBalanceAsync(
                    operationId: command.OperationId,
                    accountId: command.AccountId,
                    amountDelta: command.AmountDelta,
                    changeLimit: false);
            }
            catch (Exception ex)
            {
                publisher.PublishEvent(new AccountBalanceChangeFailedEvent(command.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, ex.Message, command.Source));
                return CommandHandlingResult.Ok(); //means no retries required
            }
            
            _chaosKitty.Meow(command.OperationId);

            var change = new AccountBalanceChangeContract(
                id: command.OperationId,
                changeTimestamp: account.ModificationTimestamp,
                accountId: account.Id,
                clientId: account.ClientId,
                changeAmount: command.AmountDelta,
                balance: account.Balance,
                withdrawTransferLimit: account.WithdrawTransferLimit,
                comment: command.Comment,
                reasonType: Convert(command.ChangeReasonType),
                eventSourceId: command.EventSourceId,
                legalEntity: account.LegalEntity,
                auditLog: command.AuditLog,
                instrument: command.AssetPairId,
                tradingDate: command.TradingDay);

            var convertedAccount = Convert(account);

            publisher.PublishEvent(new AccountChangedEvent(change.ChangeTimestamp, command.Source, convertedAccount,
                AccountChangedEventTypeContract.BalanceUpdated, change, command.OperationId));
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles external balance changing command
        /// </summary>
        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ChangeBalanceCommand command, IEventPublisher publisher)
        {
            return await Handle(new UpdateBalanceInternalCommand(
                operationId: command.OperationId,
                accountId: command.AccountId,
                amountDelta: command.Amount,
                comment: command.Reason,
                auditLog: command.AuditLog,
                source: $"{command.ReasonType.ToString()} command",
                changeReasonType: command.ReasonType.ToType<AccountBalanceChangeReasonType>(),
                eventSourceId: command.EventSourceId,
                assetPairId: command.AssetPairId,
                tradingDay: command.TradingDay
            ), publisher);
        }

        private AccountContract Convert(IAccount account)
        {
            return _convertService.Convert<AccountContract>(account);
        }

        private AccountBalanceChangeReasonTypeContract Convert(AccountBalanceChangeReasonType reasonType)
        {
            return _convertService.Convert<AccountBalanceChangeReasonTypeContract>(reasonType);
        }
    }
}