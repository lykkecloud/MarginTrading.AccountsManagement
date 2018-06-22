using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class SendBalanceCommandsService : ISendBalanceCommandsService
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;

        public SendBalanceCommandsService(ICqrsEngine cqrsEngine, CqrsContextNamesSettings cqrsContextNamesSettings)
        {
            _cqrsEngine = cqrsEngine;
            _cqrsContextNamesSettings = cqrsContextNamesSettings;
        }

        public Task<string> ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string source, string auditLog)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new UpdateBalanceInternalCommand(
                    operationId: operationId,
                    clientId: clientId,
                    accountId: accountId,
                    amountDelta: amountDelta,
                    comment: reason,
                    auditLog: auditLog,
                    source: source,
                    changeReasonType: AccountBalanceChangeReasonType.Manual),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> WithdrawAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string auditLog)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new WithdrawCommand(
                    operationId: operationId,
                    clientId: clientId,
                    accountId: accountId,
                    amount: amountDelta,
                    comment: reason,
                    auditLog: auditLog),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> DepositAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string auditLog)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new DepositCommand(
                    operationId: operationId,
                    clientId: clientId,
                    accountId: accountId,
                    amount: amountDelta,
                    comment: reason,
                    auditLog: auditLog),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }
    }
}