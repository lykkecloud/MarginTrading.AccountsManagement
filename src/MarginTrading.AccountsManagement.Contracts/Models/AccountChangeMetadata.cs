// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Object with additional information for activities
    /// </summary>
    public class AccountChangeMetadata
    {
        /// <summary>
        /// Account state before action, that generated event
        /// </summary>
        public AccountContract PreviousAccountSnapshot { get; set; }
    }
}