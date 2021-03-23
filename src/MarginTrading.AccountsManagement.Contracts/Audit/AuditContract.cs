// Copyright (c) 2020 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.Contracts.Audit
{
    /// <summary>
    /// Contract for audit
    /// </summary>
    public class AuditContract
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Correlation Id
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Name of the user who did the change
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Type of the operation
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Type of the data
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Reference data
        /// </summary>
        public string DataReference { get; set; }

        /// <summary>
        /// The difference between old and new
        /// </summary>
        public string DataDiff { get; set; }
    }
}
