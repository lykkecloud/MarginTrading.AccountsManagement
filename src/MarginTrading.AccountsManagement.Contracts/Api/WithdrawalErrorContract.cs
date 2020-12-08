// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public enum WithdrawalErrorContract
    {
        None = 0,
        InvalidRequest,
        InvalidAccount, 
        InvalidAmount, 
        OutOfTradingHours,
        OutOfBusinessDays,
        UnknownError,
        WithdrawalDisabled
    }
}