using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitstampAdapter.Services.Settings
{
    public sealed class PublishSettings
    {
        public bool Enabled { get; set; }

        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchanger { get; set; }
    }
}