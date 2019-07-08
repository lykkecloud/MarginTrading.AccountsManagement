// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    /// <summary>
    /// The response which is produced in case of an error
    /// </summary>
    [PublicAPI]
    public class ErrorResponse
    {
        /// <summary>
        /// What happend in short user-friendly form
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Some more info mostly for the developers
        /// </summary>
        [CanBeNull] public string Details { get; set; }
        
        /// <summary>
        /// Machine-readable error code
        /// </summary>
        [CanBeNull] public string ErrorCode { get; set; }
        
        /// <summary>
        /// The name of a concrete request field the error is bound to, if any
        /// </summary>
        [CanBeNull] public string FieldName { get; set; }
    }
}