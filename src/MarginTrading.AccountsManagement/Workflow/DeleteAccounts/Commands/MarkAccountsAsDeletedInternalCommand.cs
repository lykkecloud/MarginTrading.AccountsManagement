// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands
{
    [MessagePackObject]
    public class MarkAccountsAsDeletedInternalCommand
    {
        [Key(0)]
        public string OperationId { get; set; }
        
        [Key(1)]
        public DateTime Timestamp { get; set; }
    }
}