using System;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    
    public class NegativeProtectionService : INegativeProtectionService
    {
        private readonly ISendBalanceCommandsService _sendBalanceCommandsService;
        private readonly IEventSender _eventSender;
        private readonly ISystemClock _systemClock;
        
        public NegativeProtectionService(
            ISendBalanceCommandsService sendBalanceCommandsService,
            IEventSender eventSender,
            ISystemClock systemClock)
        {
            _sendBalanceCommandsService = sendBalanceCommandsService;
            _eventSender = eventSender;
            _systemClock = systemClock;
        }
        
        public async Task CheckAsync(string correlationId, string causationId, IAccount account)
        {
            if (account == null || account.Balance >= 0)
                return;
            
            //idempotency is satisfied at source sagas
            
            await _sendBalanceCommandsService.ChargeManuallyAsync(
                clientId: account.ClientId,
                accountId: account.Id,
                amountDelta: Math.Abs(account.Balance),
                operationId: $"{causationId}-negative-protection",
                reason: "Negative protection",
                source: nameof(NegativeProtectionService),
                auditLog: null,
                type: AccountBalanceChangeReasonType.CompensationPayments,
                eventSourceId: correlationId,
                assetPairId: null, 
                tradingDate: _systemClock.UtcNow.UtcDateTime
            );

            await _eventSender.SendNegativeProtectionMessage(
                correlationId: correlationId,
                causationId: causationId,
                clientId: account.ClientId,
                accountId: account.Id,
                amount: Math.Abs(account.Balance)
            );
        }
    }
}