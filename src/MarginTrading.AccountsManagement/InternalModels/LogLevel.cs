using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    /// <summary>
    /// Specifies log level
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warning = 1 << 1,
        Error = 1 << 2,
        FatalError = 1 << 3,
        Monitoring = 1 << 4,
        All = Info | Warning | Error | FatalError | Monitoring
    }
}