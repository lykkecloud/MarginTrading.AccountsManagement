// Copyright (c) 2019 Lykke Corp.

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// What happened to the account
    /// </summary>
    public enum AccountChangedEventTypeContract
    {
        /// <summary>
        /// Well, the account was created
        /// </summary>
        Created = 1,
        
        /// <summary>
        /// Arbitrary account field was updated
        /// </summary>
        Updated = 2,
        
        /// <summary>
        /// The balance was updated
        /// </summary>
        BalanceUpdated = 4,
        
        /// <summary>
        /// The account was deleted
        /// </summary>
        Deleted = 6,
    }
}