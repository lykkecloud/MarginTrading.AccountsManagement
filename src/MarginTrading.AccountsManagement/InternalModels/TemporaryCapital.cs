using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonValueSerializer]
    public class TemporaryCapital
    {
        public string Id { get; set; }
        
        public decimal Amount { get; set; }
    }
}