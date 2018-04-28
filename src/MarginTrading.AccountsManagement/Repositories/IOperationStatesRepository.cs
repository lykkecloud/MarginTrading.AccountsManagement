using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    internal interface IOperationStatesRepository
    {
        Task SetStateAsync(string operationName, string id, string state);

        Task<bool> TryInsertOrModifyAsync(string operationName, string id, Func<string, Task<string>> modify);
        Task<bool> TryInsertAsync(string operationName, string id, string state);
    }
}