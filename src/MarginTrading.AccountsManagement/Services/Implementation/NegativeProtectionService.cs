// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using static System.Math;
using System.Threading.Tasks;
using Common;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.NegativeProtection;
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
        
        public async Task<decimal?> CheckAsync(string operationId, string accountId, decimal newBalance, decimal changeAmount)
        {
            if (newBalance >= 0 || changeAmount > 0)
                return null;

            // If the balance had already been negative before change happened we compensate only changeAmount
            // If the balance had been positive before change happened we compensate the difference 
            var compensationAmount = newBalance < changeAmount ? Abs(changeAmount) : Abs(newBalance); 

            if (_negativeProtectionAutoCompensation)
            {
                var auditLog = new {CreatedAt = DateTime.UtcNow};
                
                await _sendBalanceCommandsService.ChargeManuallyAsync(
                    accountId,
                    compensationAmount, 
                    $"{operationId}-negative-protection",
                    "Negative protection",
                    NegativeProtectionSaga.CompensationTransactionSource,
                    auditLog.ToJson(),
                    AccountBalanceChangeReasonType.CompensationPayments,
                    operationId,
                    null,
                    _systemClock.UtcNow.UtcDateTime
                );
            }

            return compensationAmount;
        }
    }
}