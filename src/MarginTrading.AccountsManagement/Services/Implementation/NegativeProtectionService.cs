// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;
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
        
        public async Task<decimal?> CheckAsync(string operationId, string accountId, decimal balance)
        {
            if (balance >= 0)
                return null;
            
            var amount = Math.Abs(balance);

            if (_negativeProtectionAutoCompensation)
            {
                await _sendBalanceCommandsService.ChargeManuallyAsync(
                    accountId,
                    amount,
                    $"{operationId}-negative-protection",
                    "Negative protection",
                    nameof(NegativeProtectionService),
                    null,
                    AccountBalanceChangeReasonType.CompensationPayments,
                    operationId,
                    null,
                    _systemClock.UtcNow.UtcDateTime
                );
            }

            return amount;
        }
    }
}