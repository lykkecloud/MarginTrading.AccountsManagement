// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    /// <summary>
    /// Manage user temporary capital
    /// </summary>
    [Authorize]
    [Route("api/temporary-capital")]
    public class TemporaryCapitalController : Controller, ITemporaryCapitalController
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly IAccuracyService _accuracyService;
        
        public TemporaryCapitalController(
            IAccountManagementService accountManagementService,
            IAccuracyService accuracyService)
        {
            _accountManagementService = accountManagementService;
            _accuracyService = accuracyService;
        }

        /// <summary>
        /// Start give temporary capital to investor account operation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Operation ID</returns>
        [HttpPost]
        public async Task<string> GiveTemporaryCapital([FromBody][NotNull] GiveTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            var account = await _accountManagementService.EnsureAccountValidAsync(request.AccountId
                .RequiredNotNullOrWhiteSpace(nameof(request.AccountId)));

            var amount = await _accuracyService.ToAccountAccuracy(
                request.Amount.RequiredGreaterThan(0, nameof(request.Amount)),
                account.BaseAssetId, nameof(GiveTemporaryCapital));

            return await _accountManagementService.StartGiveTemporaryCapital(
                request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                account.Id,
                amount,
                request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)), 
                request.Comment,
                request.AdditionalInfo
            );
        }

        /// <summary>
        /// Revoke temporary capital previously given to an account. One transaction at a time or altogether.
        /// </summary>
        /// <param name="request"></param>
        [HttpDelete]
        public async Task<string> RevokeTemporaryCapital([FromBody][NotNull] RevokeTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            var account = await _accountManagementService.EnsureAccountValidAsync(request.AccountId
                .RequiredNotNullOrWhiteSpace(nameof(request.AccountId)));

            return await _accountManagementService.StartRevokeTemporaryCapital(
                request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                request.RevokeEventSourceId, 
                request.Comment, 
                request.AdditionalInfo);
        }
    }
}