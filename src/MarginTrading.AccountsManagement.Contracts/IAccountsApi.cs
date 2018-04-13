using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;
using Refit;

namespace MarginTrading.AccountsManagement.Contracts
{
    [PublicAPI]
    public interface IAccountsApi
    {
        [Get("/api/accounts/")]
        Task<List<AccountContract>> List();

        [Get("/api/accounts/{accountId}")]
        [ItemCanBeNull]
        Task<AccountContract> Get(string accountId);

        [Post("/api/accounts/")]
        Task<AccountContract> Insert([Body] AccountContract account);

        [Put("/api/accounts/{accountId}")]
        Task<AccountContract> Update(string accountId, [Body] AccountContract account);

        [Delete("/api/accounts/{accountId}")]
        Task Delete([NotNull] string accountId);
    }
}