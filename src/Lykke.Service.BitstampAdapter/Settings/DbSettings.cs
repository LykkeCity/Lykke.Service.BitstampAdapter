using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitstampAdapter.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
