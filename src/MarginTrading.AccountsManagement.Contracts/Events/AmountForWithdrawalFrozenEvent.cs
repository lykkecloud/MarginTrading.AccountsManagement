// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFrozenEvent : AccountBalanceBaseMessage
    {
        public AmountForWithdrawalFrozenEvent(string operationId, DateTime eventTimestamp, 
            string accountId, decimal amount, string reason)
            : base(operationId, eventTimestamp, accountId, amount, reason)
        {
        }
    }
}