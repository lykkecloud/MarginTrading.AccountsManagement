using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/balance-history")]
    public class AccountBalanceHistoryController : Controller, IAccountBalanceHistoryApi
    {
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly IConvertService _convertService;

        public AccountBalanceHistoryController(IAccountBalanceChangesRepository accountBalanceChangesRepository,
            IConvertService convertService)
        {
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _convertService = convertService;
        }

        [Route("")]
        [HttpGet]
        public async Task<Dictionary<string, AccountBalanceChangeContract[]>> ByAccounts(string[] accountIds,
            DateTime? from = null, DateTime? to = null)
        {
            var data = await _accountBalanceChangesRepository.GetAsync(accountIds, @from?.ToUniversalTime(),
                to?.ToUniversalTime());
            return data.GroupBy(i => i.AccountId).ToDictionary(g => g.Key, g => g.Select(Convert).ToArray());
        }

        private AccountBalanceChangeContract Convert(IAccountBalanceChange arg)
        {
            return new AccountBalanceChangeContract(arg.Id, arg.ChangeTimestamp, arg.AccountId, arg.ClientId,
                arg.ChangeAmount, arg.Balance, arg.WithdrawTransferLimit, arg.Comment, 
                Enum.Parse<AccountBalanceChangeReasonTypeContract>(arg.ReasonType.ToString()),
                arg.EventSourceId, arg.LegalEntity, arg.AccountId);
        }
    }
}