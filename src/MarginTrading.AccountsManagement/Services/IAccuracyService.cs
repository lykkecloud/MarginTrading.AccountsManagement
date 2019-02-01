using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAccuracyService
    {
        Task<decimal> ToAccountAccuracy(decimal amount, string accountBaseAsset, string operationName);
    }
}