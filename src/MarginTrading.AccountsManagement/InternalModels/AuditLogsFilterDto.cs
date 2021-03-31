// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.AccountsManagement.Dal.Common;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class AuditLogsFilterDto
    {

        public string CorrelationId { get; set; }

        public string UserName { get; set; }

        public string ActionType { get; set; }

        public string[] DataTypes { get; set; }

        public string ReferenceId { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public AuditLogsFilterDto AddSqlLikeWildcards()
        {
            return new AuditLogsFilterDto
            {
                ActionType = ActionType,
                CorrelationId = CorrelationId,
                DataTypes = DataTypes,
                ReferenceId = ReferenceId.AddLikeWildcards(),
                UserName = UserName.AddLikeWildcards(),
                EndDateTime = EndDateTime,
                StartDateTime = StartDateTime
            };
        }
    }
}