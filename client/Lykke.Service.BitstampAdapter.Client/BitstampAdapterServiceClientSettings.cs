using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BitstampAdapter.Client
{
    /// <summary>
    /// Bitstamp adapter service client settings.
    /// </summary>
    [PublicAPI]
    public class BitstampAdapterServiceClientSettings
    {
        /// <summary>
        /// Service url.
        /// </summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// The api key which used to authorize.
        /// </summary>
        public string ApiKey { get; set; }
    }
}
