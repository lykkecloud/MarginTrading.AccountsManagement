using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.SettingsService.Contracts;
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
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: account.Id,
                amount: amount,
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)), 
                comment: request.Comment,
                additionalInfo: request.AdditionalInfo
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
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                revokeEventSourceId: request.RevokeEventSourceId, 
                comment: request.Comment, 
                additionalInfo: request.AdditionalInfo);
        }
    }
}