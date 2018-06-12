using System;
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
        public Decimal Amount { get; }

        [Key(3)]
        public string OperationId { get; }

        [Key(4)]
        public string Reason { get; }

        protected AccountBalanceMessageBase(string clientId, string accountId, Decimal amount, string operationId, string reason)
        {
            string str1 = clientId;
            if (str1 == null)
                throw new ArgumentNullException(nameof (clientId));
            this.ClientId = str1;
            string str2 = accountId;
            if (str2 == null)
                throw new ArgumentNullException(nameof (accountId));
            this.AccountId = str2;
            this.Amount = amount;
            string str3 = operationId;
            if (str3 == null)
                throw new ArgumentNullException(nameof (operationId));
            this.OperationId = str3;
            string str4 = reason;
            if (str4 == null)
                throw new ArgumentNullException(nameof (reason));
            this.Reason = str4;
        }
    }
}