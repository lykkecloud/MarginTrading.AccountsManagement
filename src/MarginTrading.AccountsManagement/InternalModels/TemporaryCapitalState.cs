// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.InternalModels
{
    public enum TemporaryCapitalState
    {
        Initiated,
        
        Started,
        
        ChargedOnAccount,

        Failing,
        
        Succeded,
        
        Failed,
    }
}