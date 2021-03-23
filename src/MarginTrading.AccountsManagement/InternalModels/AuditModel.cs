// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class AuditModel
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string CorrelationId { get; set; }

        public string UserName { get; set; }

        public AuditEventType Type { get; set; }

        public AuditDataType DataType { get; set; }

        public string DataReference { get; set; }

        public string DataDiff { get; set; }
    }
}