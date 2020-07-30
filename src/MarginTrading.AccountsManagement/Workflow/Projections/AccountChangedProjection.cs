// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Services;

namespace MarginTrading.AccountsManagement.Workflow.Projections
{
    public class AccountChangedProjection
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly ILog _log;

        public AccountChangedProjection(IAccountManagementService accountManagementService, ILog log)
        {
            _accountManagementService = accountManagementService;
            _log = log;
        }

        [UsedImplicitly]
        public async Task Handle(AccountChangedEvent e)
        {
            var accountId = e?.Account?.Id;

            if (string.IsNullOrEmpty(accountId))
            {
                await _log.WriteWarningAsync(
                    nameof(AccountChangedProjection), 
                    nameof(Handle), 
                    e.ToJson(),
                    "Account id is empty");
            }
            else
            {
                _accountManagementService.ClearStatsCache(accountId);
            }
        }
    }
}