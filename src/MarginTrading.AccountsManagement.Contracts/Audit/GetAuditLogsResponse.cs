// Copyright (c) 2020 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.Contracts.Audit
{
    /// <summary>
    /// Response model for audit logs
    /// </summary>
    public class GetAuditLogsResponse
    {
        /// <summary>
        /// collection of audit logs
        /// </summary>
        public IReadOnlyList<AuditContract> AuditLogs { get; set; }
    }
}
