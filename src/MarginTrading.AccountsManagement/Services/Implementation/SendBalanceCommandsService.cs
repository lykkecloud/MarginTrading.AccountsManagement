using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
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

        public Task ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string source)
        {
            source.RequiredNotNullOrWhiteSpace(nameof(source));
            _cqrsEngine.SendCommand(
                new BeginBalanceUpdateInternalCommand(clientId, accountId, amountDelta,
                    operationId ?? Guid.NewGuid().ToString(), reason, source),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.CompletedTask;
        }

        public Task WithdrawAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason)
        {
            _cqrsEngine.SendCommand(
                new BeginWithdrawalCommand(clientId, accountId, amountDelta, operationId ?? Guid.NewGuid().ToString(),
                    reason),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.CompletedTask;
        }

        public Task DepositAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason)
        {
            _cqrsEngine.SendCommand(
                new BeginDepositCommand(clientId, accountId, amountDelta, operationId ?? Guid.NewGuid().ToString(),
                    reason),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.CompletedTask;
        }
    }
}