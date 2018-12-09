using System;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class NegativeProtectionService : INegativeProtectionService
    {
        private readonly ISendBalanceCommandsService _sendBalanceCommandsService;
        private readonly ISystemClock _systemClock;
        private readonly bool _negativeProtectionAutoCompensation;
        
        public NegativeProtectionService(
            ISendBalanceCommandsService sendBalanceCommandsService,
            ISystemClock systemClock,
            AccountManagementSettings accountManagementSettings)
        {
            _sendBalanceCommandsService = sendBalanceCommandsService;
            _systemClock = systemClock;
            _negativeProtectionAutoCompensation = accountManagementSettings.NegativeProtectionAutoCompensation;
        }
        
        public async Task<decimal?> CheckAsync(string operationId, IAccount account)
        {
            if (account == null || account.Balance >= 0)
                return null;
            
            //idempotency is satisfied at source sagas
            
            var amount = Math.Abs(account.Balance);

            if (_negativeProtectionAutoCompensation)
            {
                await _sendBalanceCommandsService.ChargeManuallyAsync(
                    accountId: account.Id,
                    amountDelta: amount,
                    operationId: $"{operationId}-negative-protection",
                    reason: "Negative protection",
                    source: nameof(NegativeProtectionService),
                    auditLog: null,
                    type: AccountBalanceChangeReasonType.CompensationPayments,
                    eventSourceId: operationId,
                    assetPairId: null,
                    tradingDate: _systemClock.UtcNow.UtcDateTime
                );
            }

            return amount;
        }
    }
}