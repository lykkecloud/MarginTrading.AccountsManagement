using System;
using AutoMapper;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    public class AccountBalanceChangeEntity : IAccountBalanceChange
    {
        [IgnoreMap]
        public long Oid { get; set; }
        
        public string AccountId { get; set; }
        
        public DateTime ChangeTimestamp { get; set; }
        
        public string Id { get; set; }

        public string ClientId { get; set; }

        decimal IAccountBalanceChange.ChangeAmount => (decimal) ChangeAmount;
        public double ChangeAmount { get; set; }

        decimal IAccountBalanceChange.Balance => (decimal) Balance;
        public double Balance { get; set; }

        decimal IAccountBalanceChange.WithdrawTransferLimit => (decimal) WithdrawTransferLimit;
        public double WithdrawTransferLimit { get; set; }

        public string Comment { get; set; }

        AccountBalanceChangeReasonType IAccountBalanceChange.Type => Enum.Parse<AccountBalanceChangeReasonType>(Type); 
        public string Type { get; set; }

        public string EventSourceId { get; set; }

        public string LegalEntity { get; set; }

        public string AuditLog { get; set; }
    }
}