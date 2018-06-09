using System;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    internal class AccountBalanceChangeEntity : AzureTableEntity, IAccountBalanceChange
    {
        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }
        
        public DateTime ChangeTimestamp
        {
            get => DateTime.ParseExact(RowKey, RowKeyDateTimeFormat.Iso.ToDateTimeMask(), null); 
            set => RowKey = value.ToString(RowKeyDateTimeFormat.Iso.ToDateTimeMask());
        }
        
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

        public static string GeneratePartitionKey(string accountId)
        {
            return accountId;
        }
    }
}