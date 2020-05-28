// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class WithdrawalDepositData
    {
        public string AccountId { get; set; }
        public decimal Amount { get; set; }
        public string AuditLog { get; set; }
        public WithdrawalState State { get; set; }
        public string Comment { get; set; }

        [CanBeNull]
        public string FailReason { get; set; }
    }
}
