using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Extensions;
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
        public async Task<string> GiveTemporaryCapital([FromBody][NotNull] GiveTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            await _accountManagementService.ValidateAccountId(request.AccountId);

            return await _accountManagementService.StartGiveTemporaryCapital(
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                amount: request.Amount.RequiredGreaterThan(0, nameof(request.Amount)),
                reason: request.Reason.RequiredNotNullOrWhiteSpace(nameof(request.Reason)), 
                comment: request.Comment,
                additionalInfo: request.AdditionalInfo
            );
        }

        /// <inheritdoc cref="ITemporaryCapitalController" />
        [HttpDelete]
        public async Task<string> RevokeTemporaryCapital([FromBody][NotNull] RevokeTemporaryCapitalRequest request)
        {
            request.RequiredNotNull(nameof(request));
            await _accountManagementService.ValidateAccountId(request.AccountId);

            return await _accountManagementService.StartRevokeTemporaryCapital(
                eventSourceId: request.EventSourceId.RequiredNotNullOrWhiteSpace(nameof(request.EventSourceId)),
                accountId: request.AccountId.RequiredNotNullOrWhiteSpace(nameof(request.AccountId)),
                revokeEventSourceId: request.RevokeEventSourceId, 
                comment: request.Comment, 
                additionalInfo: request.AdditionalInfo);
        }
    }
}