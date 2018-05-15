using System;
using AzureStorage;
using Lykke.AzureStorage.Tables;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    public class AccountBalanceHistoryEntity : AzureTableEntity
    {
        /// <summary>
        /// Account id
        /// </summary>
        public string AccountId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }
        
        /// <summary>
        /// Change timestamp
        /// </summary>
        public DateTime ChangeTimestamp
        {
            get => DateTime.ParseExact(RowKey, RowKeyDateTimeFormat.Iso.ToDateTimeMask(), null); 
            set => RowKey = value.ToString(RowKeyDateTimeFormat.Iso.ToDateTimeMask());
        }
        /// <summary>
        /// Change Id 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Client id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The balance diff
        /// </summary>
        public decimal ChangeAmount { get; set; }

        /// <summary>
        /// Balance after change
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Withdraw transfer limit after change
        /// </summary>
        public decimal WithdrawTransferLimit { get; set; }

        /// <summary>
        /// Why the chhange happend in a human readable form
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Why the chhange happend 
        /// </summary>
        public AccountBalanceHistoryType Type { get; set; }

        /// <summary>
        /// Id of object which caused the change (ex. order id)
        /// </summary>
        public string EventSourceId { get; set; }

        /// <summary>
        /// Legal entity of the account
        /// </summary>
        public string LegalEntity { get; set; }

        /// <summary>
        /// Log data
        /// </summary>
        public string AuditLog { get; set; }

        public static string GeneratePartitionKey(string accountId)
        {
            return accountId;
        }
    }
}