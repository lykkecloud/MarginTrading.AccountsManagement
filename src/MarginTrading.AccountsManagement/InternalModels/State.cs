using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum State
    {
        Created = 0,
        FreezingAmount = 1,
        UpdatingBalance = 2,
        UnfreezingAmount = 3,
        Succeeded = 4,
        Failed = 5,

    }
}
