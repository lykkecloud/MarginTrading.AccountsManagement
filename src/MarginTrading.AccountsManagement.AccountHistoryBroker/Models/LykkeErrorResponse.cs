// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Models
{
    public class LykkeErrorResponse
    {
        public string ErrorMessage { get; set; }

        public override string ToString() => ErrorMessage;
    }
}