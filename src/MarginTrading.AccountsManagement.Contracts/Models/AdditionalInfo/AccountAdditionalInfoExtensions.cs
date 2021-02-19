// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.Contracts.Models.AdditionalInfo
{
    public static class AccountAdditionalInfoExtensions
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

        public static string Serialize(this AccountAdditionalInfo source)
        {
            return JsonConvert.SerializeObject(source,serializerSettings);
        }

        public static AccountAdditionalInfo Deserialize(string additionalInfo)
        {
            return JsonConvert.DeserializeObject<AccountAdditionalInfo>(additionalInfo);
        }

        public static AccountAdditionalInfo DeserializeAdditionalInfo(this AccountContract account)
        {
            return Deserialize(account.AdditionalInfo);
        }

        public static AccountAdditionalInfo DeserializeAdditionalInfo(this AccountStatContract account)
        {
            return Deserialize(account.AdditionalInfo);
        }
    }
}