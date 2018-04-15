using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class AccountEntity : AzureTableEntity
    {
        private decimal _withdrawTransferLimit;
        private decimal _balance;
        private bool _isDisabled;

        public string Id { get; set; }
        
        public string ClientId { get; set; }
        
        public string TradingConditionId { get; set; }
        
        public string BaseAssetId { get; set; }
        
        public decimal Balance 
        {
            get => _balance;
            set
            {
                _balance = value;
                MarkValueTypePropertyAsDirty(nameof(Balance));
            }
        }
        
        public decimal WithdrawTransferLimit 
        {
            get => _withdrawTransferLimit;
            set
            {
                _withdrawTransferLimit = value;
                MarkValueTypePropertyAsDirty(nameof(WithdrawTransferLimit));
            }
        }
        
        public string LegalEntity { get; set; }

        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                _isDisabled = value;
                MarkValueTypePropertyAsDirty(nameof(IsDisabled));
            }
        }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }
    }
}