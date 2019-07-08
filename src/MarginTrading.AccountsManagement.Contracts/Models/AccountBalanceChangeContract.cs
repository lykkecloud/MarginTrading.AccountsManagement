// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Contract of account balance change 
    /// </summary>
    [MessagePackObject]
    public class AccountBalanceChangeContract
    {
        public AccountBalanceChangeContract([NotNull] string id, DateTime changeTimestamp, [NotNull] string accountId,
            [NotNull] string clientId, decimal changeAmount, decimal balance, decimal withdrawTransferLimit,
            [NotNull] string comment, AccountBalanceChangeReasonTypeContract reasonType, [NotNull] string eventSourceId,
            [NotNull] string legalEntity, string auditLog, string instrument, DateTime tradingDate)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ChangeTimestamp = changeTimestamp;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            ChangeAmount = changeAmount;
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            Comment = comment;
            ReasonType = reasonType;
            EventSourceId = eventSourceId;
            LegalEntity = legalEntity;
            AuditLog = auditLog;
            Instrument = instrument;
            TradingDate = tradingDate == DateTime.MinValue ? changeTimestamp.Date : tradingDate.Date;
        }

        /// <summary>
        /// Change Id 
        /// </summary>
        [NotNull]
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
        [NotNull]
        [Key(2)]
        public string AccountId { get; }

        /// <summary>
        /// Client id
        /// </summary>
        [NotNull]
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
        [NotNull]
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
        [NotNull]
        [Key(9)]
        public string EventSourceId { get; }

        /// <summary>
        /// Legal entity of the account
        /// </summary>
        [NotNull]
        [Key(10)]
        public string LegalEntity { get; }

        /// <summary>
        /// Log data
        /// </summary>
        [CanBeNull]
        [Key(11)]
        public string AuditLog { get; }
        
        /// <summary>
        /// Instrument Id
        /// </summary>
        [CanBeNull]
        [Key(12)]
        public string Instrument { get; }
        
        /// <summary>
        /// Trading date is passed with model, if not it is set to current time
        /// </summary>
        [Key(13)]
        public DateTime TradingDate { get; }
    }
}