// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAuditRepository
    {
        void Initialize();
        Task InsertAsync(AuditModel model);
        Task<PaginatedResponse<AuditModel>> GetAll(AuditLogsFilterDto filter, int? skip, int? take);
    }
}