// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class GiveTemporaryCapitalData : OperationDataBase<TemporaryCapitalState>
    {
        public string OperationId { get; set; }

        public string AccountId { get; set; }

        public decimal Amount { get; set; }

        public string Reason { get; set; }
        
        public string Comment { get; set; }

        public string AdditionalInfo { get; set; }
        
        public string FailReason { get; set; }
    }
}