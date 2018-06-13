using System;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Models
{
    public class AccountHistory : IAccountHistory
    {
        public AccountHistory(string id, DateTime changeTimestamp, string accountId, string clientId,
            decimal changeAmount, decimal balance, decimal withdrawTransferLimit, string comment,
            AccountBalanceChangeReasonType reasonType, string eventSourceId, string legalEntity, string auditLog)
        {
            Id = id;
            ChangeTimestamp = changeTimestamp;
            AccountId = accountId;
            ClientId = clientId;
            ChangeAmount = changeAmount;
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            Comment = comment;
            ReasonType = reasonType;
            EventSourceId = eventSourceId;
            LegalEntity = legalEntity;
            AuditLog = auditLog;
        }

        public string Id { get; }
        public DateTime ChangeTimestamp { get; }
        public string AccountId { get; }
        public string ClientId { get; }
        public decimal ChangeAmount { get; }
        public decimal Balance { get; }
        public decimal WithdrawTransferLimit { get; }
        public string Comment { get; }
        public AccountBalanceChangeReasonType ReasonType { get; }
        public string EventSourceId { get; }
        public string LegalEntity { get; }
        public string AuditLog { get; }
    }
}

