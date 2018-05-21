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

        public UpdateBalanceInternalCommand(string operationId, string clientId,
            string accountId, decimal amountDelta, string comment, string auditLog,
            string source, AccountBalanceChangeReasonType changeReasonType)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AmountDelta = amountDelta;
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
            AuditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ChangeReasonType = changeReasonType.RequiredEnum(nameof(changeReasonType));
        }
    }
}