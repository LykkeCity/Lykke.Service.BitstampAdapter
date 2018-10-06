using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer
{
    public class WalletAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
