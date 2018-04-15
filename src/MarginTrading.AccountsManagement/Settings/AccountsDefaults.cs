using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class AccountsDefaults
    {
        [Optional]
        public string AccountIdPrefix { get; set; }
        
        [Optional]
        public decimal DefaultBalance { get; set; }
        
        [Optional]
        public bool BalanceResetIsEnabled { get; set; }
    }
}