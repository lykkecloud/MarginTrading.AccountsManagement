using Lykke.AzureStorage.Tables.Entity.Annotation;
using MessagePack;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonValueSerializer]
    [MessagePackObject]
    public class TemporaryCapital
    {
        [Key(0)]
        public string Id { get; set; }
        
        [Key(1)]
        public decimal Amount { get; set; }
    }
}