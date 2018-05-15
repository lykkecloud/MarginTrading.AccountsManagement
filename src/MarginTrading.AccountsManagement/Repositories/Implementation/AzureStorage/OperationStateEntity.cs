using Lykke.AzureStorage.Tables;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    public class OperationStateEntity : AzureTableEntity
    {
        public string OperationName
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }
        
        public string Id
        {
            get => RowKey;
            set => RowKey = value;
        }
        
        public string State { get; set; }

        public static string GeneratePartitionKey(string operationName)
        {
            return operationName;
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }
    }
}