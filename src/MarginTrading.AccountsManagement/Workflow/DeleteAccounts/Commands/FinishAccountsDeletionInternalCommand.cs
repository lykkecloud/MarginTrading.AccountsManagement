// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands
{
    [MessagePackObject]
    public class FinishAccountsDeletionInternalCommand
    {
        [CanBeNull]
        [Key(0)]
        public string OperationId { get; set; }
        
        [Key(1)]
        public DateTime Timestamp { get; set; }
    }
}