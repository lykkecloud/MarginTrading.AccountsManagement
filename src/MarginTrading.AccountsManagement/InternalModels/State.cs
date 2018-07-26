using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.AccountsManagement.InternalModels
{
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
