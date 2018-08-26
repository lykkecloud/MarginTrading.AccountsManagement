using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class WithdrawalDepositData
    {
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public decimal Amount { get; set; }
        public string AuditLog { get; set; }
        public State State { get; set; }
        public string Comment { get; set; }

        [CanBeNull]
        public string FailReason { get; set; }
    }
}
