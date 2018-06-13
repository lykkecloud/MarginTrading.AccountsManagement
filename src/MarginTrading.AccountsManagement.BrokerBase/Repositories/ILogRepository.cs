using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.BrokerBase.Repositories
{
    public interface ILogRepository
    {
        Task Insert(LogEntity log);
    }
}