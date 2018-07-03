using System;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands
{
    [MessagePackObject]
    internal class UpdateBalanceInternalCommand
    {
        [Key(0)]
        public string OperationId { get; }

        [Key(1)]
        public string ClientId { get; }

        [Key(2)]
        public string AccountId { get; }

        [Key(3)]
        public decimal AmountDelta { get; }

        [Key(4)]
        public string Comment { get; }

        [Key(5)]
        public string AuditLog { get; }

        [Key(6)]
        public string Source { get; }

        [Key(7)]
        public AccountBalanceChangeReasonType ChangeReasonType { get; }
        
        /// <summary>
        /// Event source ID (order, position, trade, etc).
        /// </summary>
        [Key(8)]
        public string EventSourceId { get; }
        
        /// <summary>
        /// Asset Pair ID (if can be found any)
        /// </summary>
        [Key(9)]
        public string AssetPairId { get; }
        
        [Key(10)]
        public DateTime TradingDay { get; }

        public UpdateBalanceInternalCommand(string operationId, string clientId,
            string accountId, decimal amountDelta, string comment, string auditLog,
            string source, AccountBalanceChangeReasonType changeReasonType, string eventSourceId, string assetPairId, DateTime tradingDay)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            ClientId = clientId;
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AmountDelta = amountDelta;
            Comment = comment;
            AuditLog = auditLog;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            EventSourceId = eventSourceId;
            AssetPairId = assetPairId;
            TradingDay = tradingDay;
            ChangeReasonType = changeReasonType.RequiredEnum(nameof(changeReasonType));
        }
    }
}