using System.Threading.Tasks;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories
{
    public interface IAccountHistoryRepository
    {
        Task InsertOrReplaceAsync(IAccountHistory entity);
    }
}