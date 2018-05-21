using System;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Contract of account balance change 
    /// </summary>
    [MessagePackObject]
    public class AccountBalanceChangeContract
    {
        /// <inheritdoc />
        public AccountBalanceChangeContract(string id, DateTime changeTimestamp, string accountId, string clientId,
            decimal changeAmount, decimal balance, decimal withdrawTransferLimit, string comment,
            AccountBalanceChangeReasonTypeContract reasonType, string eventSourceId, string legalEntity, string auditLog)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ChangeTimestamp = changeTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            ChangeAmount = changeAmount;
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
            ReasonType = reasonType;
            EventSourceId = eventSourceId ?? throw new ArgumentNullException(nameof(eventSourceId));
            LegalEntity = legalEntity ?? throw new ArgumentNullException(nameof(legalEntity));
            AuditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        }

        /// <summary>
        /// Change Id 
        /// </summary>
        [Key(0)]
        public string Id { get; }

        /// <summary>
        /// Change timestamp
        /// </summary>
        [Key(1)]
        public DateTime ChangeTimestamp { get; }

        /// <summary>
        /// Account id
        /// </summary>
        [Key(2)]
        public string AccountId { get; }

        /// <summary>
        /// Client id
        /// </summary>
        [Key(3)]
        public string ClientId { get; }

        /// <summary>
        /// The balance diff
        /// </summary>
        [Key(4)]
        public decimal ChangeAmount { get; }

        /// <summary>
        /// Balance after change
        /// </summary>
        [Key(5)]
        public decimal Balance { get; }

        /// <summary>
        /// Withdraw transfer limit after change
        /// </summary>
        [Key(6)]
        public decimal WithdrawTransferLimit { get; }

        /// <summary>
        /// Why the change happend in a human readable form
        /// </summary>
        [Key(7)]
        public string Comment { get; }

        /// <summary>
        /// Why the chhange happend 
        /// </summary>
        [Key(8)]
        public AccountBalanceChangeReasonTypeContract ReasonType { get; }

        /// <summary>
        /// Id of object which caused the change (ex. order id)
        /// </summary>
        [Key(9)]
        public string EventSourceId { get; }

        /// <summary>
        /// Legal entity of the account
        /// </summary>
        [Key(10)]
        public string LegalEntity { get; }

        /// <summary>
        /// Log data
        /// </summary>
        [Key(11)]
        public string AuditLog { get; }
    }
}