﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Contracts
{
    /// <summary>Response which holds error code</summary>
    public class ErrorCodeResponse<T>
    {
        /// <summary>Error code</summary>
        public T ErrorCode { get; set; }
    }
}