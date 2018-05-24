using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.AzureStorage
{
    public class OperationExecutionInfoEntity : AzureTableEntity
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
        
        [JsonValueSerializer]
        public object Data { get; set; }

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