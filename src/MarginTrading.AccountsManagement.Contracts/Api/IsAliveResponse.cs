using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    [PublicAPI]
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public bool IsDebug { get; set; }
        public string Name { get; set; }
    }
}