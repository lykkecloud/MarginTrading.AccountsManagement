// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Refit;

namespace MarginTrading.AccountsManagement.Contracts.Audit
{
    /// <summary>
    /// Audit lof request details
    /// </summary>
    [PublicAPI]
    public class GetAuditLogsRequest
    {
        /// <summary>
        /// The correlation id to filter upon
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// The user name or part of user name to filter upon
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The action type to filter upon
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AuditEventType? ActionType { get; set; }

        /// <summary>
        /// The audit log data types to filter upon
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [Query(CollectionFormat.Multi)]
        public AuditDataType[] DataTypes { get; set; }

        /// <summary>
        /// The reference id to filter upon
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// The start date and time audit log records will be filtered upon (equal or greater)
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// The end date and time audit log records will be filtered upon (equal or less)
        /// </summary>
        public DateTime? EndDateTime { get; set; }
    }
}