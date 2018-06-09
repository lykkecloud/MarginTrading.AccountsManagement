using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    public class OperationExecutionInfoEntity : IOperationExecutionInfo<object>
    {
        public string Version { get; set; }
        
        public string OperationName { get; set; }
        
        public string Id { get; set; }
        
        object IOperationExecutionInfo<object>.Data => JsonConvert.DeserializeObject<object>(Data);
        public string Data { get; set; }
        
    }
}