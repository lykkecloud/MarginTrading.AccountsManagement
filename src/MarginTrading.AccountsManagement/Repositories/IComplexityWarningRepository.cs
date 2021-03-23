using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IComplexityWarningRepository
    {
        void Initialize();
        Task<ComplexityWarningState> GetOrCreate(string accountId, Func<ComplexityWarningState> factory);
        Task Save(ComplexityWarningState entity);
        Task<IEnumerable<ComplexityWarningState>> GetExpired(DateTime timestamp);
    }
}
