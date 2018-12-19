using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    /// <inheritdoc cref="ITemporaryCapitalController" />
    [Route("api/temporary-capital")]
    public class TemporaryCapitalController : Controller, ITemporaryCapitalController
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        
        public TemporaryCapitalController(
            IAccountManagementService accountManagementService,
            IAccountBalanceChangesRepository accountBalanceChangesRepository)
        {
            _accountManagementService = accountManagementService;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
        }

        /// <inheritdoc cref="ITemporaryCapitalController" />
        [HttpPost]
        public async Task<string> GiveTemporaryCapital(GiveTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            await ValidateAccountId(request.AccountId);

            return await _accountManagementService.StartGiveTemporaryCapital(
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: request.AccountId,
                amount: request.Amount.RequiredGreaterThan(0, nameof(request.Amount)),
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)),
                auditLog: new
                {
                    InitiatedBy = request.InitiatedBy.RequiredNotNullOrWhiteSpace(nameof(request.InitiatedBy)),
                    ArbitratedBy = request.ArbitratedBy.RequiredNotNullOrWhiteSpace(nameof(request.ArbitratedBy)),
                    Comment = request.Comment,
                }.ToJson()
            );
        }

        /// <inheritdoc cref="ITemporaryCapitalController" />
        [HttpDelete]
        public async Task<string> RevokeTemporaryCapital(RevokeTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            await ValidateAccountId(request.AccountId);

            return await _accountManagementService.StartRevokeTemporaryCapital(
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: request.AccountId,
                revokeEventSourceId: request.RevokeEventSourceId, 
                auditLog: new { RevokedBy = request.RevokedBy }.ToJson());
        }

        /// <inheritdoc cref="ITemporaryCapitalController" />
        [HttpGet("{accountId}")]
        public async Task<List<AccountBalanceChangeContract>> ListTemporaryCapital(string accountId,
            [FromQuery] DateTime? @from = null, [FromQuery] DateTime? to = null)
        {
            var data = await _accountBalanceChangesRepository.GetAsync(accountId, @from?.ToUniversalTime(),
                to?.ToUniversalTime(), AccountBalanceChangeReasonType.TemporaryCashAdjustment);
            return data.Select(Convert).ToList();   
        }

        private async Task ValidateAccountId(string accountId)
        {
            var account = await _accountManagementService.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new ArgumentException($"Account {accountId} does not exist");
            }
        }

        private AccountBalanceChangeContract Convert(IAccountBalanceChange arg)
        {
            return new AccountBalanceChangeContract(arg.Id, arg.ChangeTimestamp, arg.AccountId, arg.ClientId,
                arg.ChangeAmount, arg.Balance, arg.WithdrawTransferLimit, arg.Comment, 
                Enum.Parse<AccountBalanceChangeReasonTypeContract>(arg.ReasonType.ToString()),
                arg.EventSourceId, arg.LegalEntity, arg.AuditLog, arg.Instrument, arg.TradingDate);
        }
    }
}