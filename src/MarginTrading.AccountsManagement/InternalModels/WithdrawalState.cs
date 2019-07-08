// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WithdrawalState
    {
        Created = 0,
        FreezingAmount = 1,
        UpdatingBalance = 2,
        UnfreezingAmount = 3,
        Succeeded = 4,
        Failed = 5,
    }
}
