// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class RevokeTemporaryCapitalData : OperationDataBase<TemporaryCapitalState>
    {
        public string OperationId { get; set; }

        public string EventSourceId { get; set; }

        public string AccountId { get; set; }
        
        public string RevokeEventSourceId { get; set; }

        public string Comment { get; set; }

        public string AdditionalInfo { get; set; }
        
        public string FailReason { get; set; }
        
        public List<TemporaryCapital> RevokedTemporaryCapital { get; set; }
    }
}