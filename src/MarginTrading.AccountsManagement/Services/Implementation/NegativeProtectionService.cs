// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
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
            
            var amount = Math.Abs(account.Balance);

            if (_negativeProtectionAutoCompensation)
            {
                var auditLog = new {CreatedAt = DateTime.UtcNow};
                
                await _sendBalanceCommandsService.ChargeManuallyAsync(
                    account.Id,
                    amount,
                    $"{operationId}-negative-protection",
                    "Negative protection",
                    nameof(NegativeProtectionService),
                    auditLog.ToJson(),
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