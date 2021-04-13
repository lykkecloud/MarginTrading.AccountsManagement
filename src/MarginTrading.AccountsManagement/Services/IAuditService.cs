// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAuditService
    {
        Task<PaginatedResponse<AuditModel>> GetAll(AuditLogsFilterDto filter, int? skip, int? take);

        Task TryAuditTradingConditionUpdate(string correlationId,
            string userName,
            string clientId,
            string newTradingConditionId,
            string oldTradingConditionId);
    }
}