// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class DeleteAccountsData : OperationDataBase<DeleteAccountsState>
    {
        public string OperationId { get; set; }

        public List<string> AccountIds { get; set; }
        
        public string Comment { get; set; }

        public Dictionary<string, string> FailedAccountIds { get; } = new Dictionary<string, string>();
        
        public List<string> GetAccountsToDelete() => AccountIds.Except(FailedAccountIds.Keys).ToList();

        public void AddFailedIfNotExist(Dictionary<string, string> failedAccountIds)
        {
            foreach (var (accountId, reason) in failedAccountIds)
            {
                if (FailedAccountIds.ContainsKey(accountId))
                {
                    continue;
                }
                    
                FailedAccountIds.Add(accountId, $"[{State.ToString()}]: {reason}");
            }
        }
    }
}