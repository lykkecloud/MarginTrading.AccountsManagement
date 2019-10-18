// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class WithdrawalResponse
    {
        public string OperationId { get; set; }
        
        public decimal Amount { get; set; }
        
        public WithdrawalErrorContract Error { get; set; }
        
        public string ErrorDetails { get; set; }
    }
}