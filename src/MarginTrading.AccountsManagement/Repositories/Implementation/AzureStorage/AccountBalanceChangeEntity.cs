using System;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
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

        private decimal _changeAmount;
        public decimal ChangeAmount
        {
            get => _changeAmount;
            set
            {
                _changeAmount = value;
                MarkValueTypePropertyAsDirty(nameof(ChangeAmount));
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                MarkValueTypePropertyAsDirty(nameof(Balance));
            }
        }

        private decimal _withdrawTransferLimit;
        public decimal WithdrawTransferLimit
        {
            get => _withdrawTransferLimit;
            set
            {
                _withdrawTransferLimit = value;
                MarkValueTypePropertyAsDirty(nameof(WithdrawTransferLimit));
            }
        }

        public string Comment { get; set; }

        AccountBalanceChangeReasonType IAccountBalanceChange.ReasonType => Enum.Parse<AccountBalanceChangeReasonType>(ReasonType);
        public string ReasonType { get; set; }
        
        public string EventSourceId { get; set; }

        public string LegalEntity { get; set; }

        public string AuditLog { get; set; }
        
        public string Instrument { get; set; }
        
        public DateTime TradingDate { get; set; }

        public static string GeneratePartitionKey(string accountId)
        {
            return accountId;
        }
    }
}