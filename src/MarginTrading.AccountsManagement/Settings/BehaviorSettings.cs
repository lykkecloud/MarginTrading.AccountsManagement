// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class BehaviorSettings
    {
        [Optional] public string AccountIdPrefix { get; set; }

        [Optional] public decimal DefaultBalance { get; set; }

        [Optional] public bool BalanceResetIsEnabled { get; set; }

        [Optional] public bool DefaultWithdrawalIsEnabled { get; set; } = true;
    }
}