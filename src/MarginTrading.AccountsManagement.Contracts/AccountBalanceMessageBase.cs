using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts
{
    [MessagePackObject(false)]
    public abstract class AccountBalanceMessageBase
    {
        [Key(0)]
        public string ClientId { get; }

        [Key(1)]
        public string AccountId { get; }

        [Key(2)]
        public decimal Amount { get; }

        [Key(3)]
        public string OperationId { get; }

        [Key(4)]
        public string Reason { get; }

        protected AccountBalanceMessageBase([NotNull] string clientId, [NotNull] string accountId, decimal amount, 
            [NotNull] string operationId, [NotNull] string reason)
        {
            this.ClientId = clientId ?? throw new ArgumentNullException(nameof (clientId));
            this.AccountId = accountId ?? throw new ArgumentNullException(nameof (accountId));
            this.Amount = amount;
            this.OperationId = operationId ?? throw new ArgumentNullException(nameof (operationId));
            this.Reason = reason ?? throw new ArgumentNullException(nameof (reason));
        }
    }
}