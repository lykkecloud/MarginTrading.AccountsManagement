using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/balance-history")]
    public class AccountBalanceHistoryController : Controller, IAccountBalanceHistoryApi
    {
        private readonly IAccountBalanceHistoryRepository _accountBalanceHistoryRepository;
        private readonly IConvertService _convertService;

        public AccountBalanceHistoryController(IAccountBalanceHistoryRepository accountBalanceHistoryRepository,
            IConvertService convertService)
        {
            _accountBalanceHistoryRepository = accountBalanceHistoryRepository;
            _convertService = convertService;
        }

        [Route("")]
        [HttpGet]
        public async Task<Dictionary<string, AccountHistoryContract[]>> ByAccounts(string[] accountIds,
            DateTime? from = null, DateTime? to = null)
        {
            return (await _accountBalanceHistoryRepository.GetAsync(accountIds, from?.ToUniversalTime(),
                    to?.ToUniversalTime())).GroupBy(i => i.AccountId)
                .ToDictionary(g => g.Key, g => g.Select(Convert).ToArray());
        }

        private AccountHistoryContract Convert(AccountBalanceHistory arg)
        {
            return _convertService.Convert<AccountHistoryContract>(arg);
        }
    }
}