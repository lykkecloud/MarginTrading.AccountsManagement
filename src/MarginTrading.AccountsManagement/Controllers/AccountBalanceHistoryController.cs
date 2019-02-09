using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Controllers
{
    /// <inheritdoc cref="IAccountBalanceHistoryApi" />
    [Route("api/balance-history")]
    public class AccountBalanceHistoryController : Controller, IAccountBalanceHistoryApi
    {
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly ISystemClock _systemClock;

        public AccountBalanceHistoryController(
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            ISystemClock systemClock)
        {
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _systemClock = systemClock;
        }

        /// <inheritdoc cref="IAccountBalanceHistoryApi" />
        [Route("by-account/{accountId}")]
        [HttpGet]
        public async Task<Dictionary<string, AccountBalanceChangeContract[]>> ByAccount(
            string accountId,
            [FromQuery] DateTime? @from = null, 
            [FromQuery] DateTime? to = null,
            [FromQuery] AccountBalanceChangeReasonTypeContract? reasonType = null)
        {
            var data = await _accountBalanceChangesRepository.GetAsync(
                accountId, 
                from: @from?.ToUniversalTime(),
                to: to?.ToUniversalTime(),
                reasonType: reasonType?.ToType<AccountBalanceChangeReasonType>());
            
            return data.GroupBy(i => i.AccountId).ToDictionary(g => g.Key, g => g.Select(Convert).ToArray());
        }

        /// <inheritdoc cref="IAccountBalanceHistoryApi" />
        [Route("{accountId}")]
        [HttpGet]
        public async Task<AccountBalanceChangeContract[]> ByAccountAndEventSource(
            string accountId, 
            [FromQuery] string eventSourceId = null)
        {
            var data = await _accountBalanceChangesRepository.GetAsync(accountId, eventSourceId);

            return data.Select(Convert).ToArray();
        }

        /// <inheritdoc cref="IAccountBalanceHistoryApi" />
        [Route("~/api/balance/{accountId}")]
        [HttpGet]
        public async Task<decimal> GetBalanceOnDate([FromRoute] string accountId, [FromQuery] DateTime? date)
        {
            var targetDate = date ?? _systemClock.UtcNow.UtcDateTime.Date;
            
            return await _accountBalanceChangesRepository.GetBalanceAsync(accountId, targetDate);
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