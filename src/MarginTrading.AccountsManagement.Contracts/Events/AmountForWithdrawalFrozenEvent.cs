// Copyright (c) 2019 Lykke Corp.

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