using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    public class ClientSettings
    {
        //[HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
        
        [Optional]
        public string ApiKey { get; set; }
    }
}