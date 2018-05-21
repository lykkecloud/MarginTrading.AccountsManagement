using System;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Starts deposit operation
    /// </summary>
    [MessagePackObject]
    public class DepositCommand
    {
        [Key(0)]
        public string OperationId { get; }

        [Key(1)]
        public string ClientId { get; }

        [Key(2)]
        public string AccountId { get; }

        [Key(3)]
        public decimal Amount { get; }

        [Key(4)]
        public string AuditLog { get; }

        public DepositCommand(string operationId, string clientId, string accountId, decimal amount, string auditLog)
        {
            OperationId = operationId;
            ClientId = clientId;
            AccountId = accountId;
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "");

            Amount = amount;
            AuditLog = auditLog;
        }
    }
}