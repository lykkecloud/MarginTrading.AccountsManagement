using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Repositories.Implementation.SQL;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface ILogRepository
    {
        Task Insert(LogEntity log);
    }
}