// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts.Api;
using Refit;

namespace MarginTrading.AccountsManagement.Contracts
{
    /// <summary>
    /// Manage user temporary capital
    /// </summary>
    public interface ITemporaryCapitalController
    {
        /// <summary>
        /// Start give temporary capital to investor account operation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Operation ID</returns>
        [Post("/api/temporary-capital")]
        Task<string> GiveTemporaryCapital([Body] GiveTemporaryCapitalRequest request);

        /// <summary>
        /// Revoke temporary capital previously given to an account. One transaction at a time or altogether.
        /// </summary>
        /// <param name="request"></param>
        [Delete("/api/temporary-capital")]
        Task<string> RevokeTemporaryCapital([Body] RevokeTemporaryCapitalRequest request);
    }
}