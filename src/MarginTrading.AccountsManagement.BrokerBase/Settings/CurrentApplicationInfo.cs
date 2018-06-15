namespace MarginTrading.AccountsManagement.BrokerBase.Settings
{
    public class CurrentApplicationInfo
    {
        public CurrentApplicationInfo(string applicationVersion, string applicationName)
        {
            ApplicationVersion = applicationVersion;
            ApplicationName = applicationName;
        }
        public string ApplicationVersion { get; }
        public string ApplicationName { get; }

        public string ApplicationFullName => $"{ApplicationName}:{ApplicationVersion}";
    }
}