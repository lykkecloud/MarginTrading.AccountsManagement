// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    /// <inheritdoc/>
    public class CqrsSender : ICqrsSender
    {
        [NotNull] public ICqrsEngine CqrsEngine { get; set; }//property injection
        [NotNull] private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;

        public CqrsSender([NotNull] CqrsContextNamesSettings cqrsContextNamesSettings)
        {
            _cqrsContextNamesSettings = cqrsContextNamesSettings ??
                                        throw new ArgumentNullException(nameof(cqrsContextNamesSettings));
        }
        
        public void SendCommandToSelf<T>(T command)
        {
            CqrsEngine.SendCommand(command, _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
        }

        public void PublishEvent<T>(T ev, string boundedContext = null)
        {
            try
            {
                CqrsEngine.PublishEvent(ev, boundedContext ?? _cqrsContextNamesSettings.AccountsManagement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}