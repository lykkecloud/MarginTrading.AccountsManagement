using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.AccountsManagement.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller, IAccountsApi
    {
        [HttpGet]
        [Route("")]
        public Task<List<AccountContract>> List()
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [Route("{accountId}")]
        public Task<AccountContract> Get(string accountId)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost]
        [Route("")]
        public Task<AccountContract> Insert([FromBody] AccountContract account)
        {
            throw new System.NotImplementedException();
        }

        [HttpPut]
        [Route("{accountId}")]
        public Task<AccountContract> Update(string accountId, [FromBody] AccountContract account)
        {
            throw new System.NotImplementedException();
        }

        [HttpDelete]
        [Route("{accountId}")]
        public Task Delete(string accountId)
        {
            throw new System.NotImplementedException();
        }
    }
}