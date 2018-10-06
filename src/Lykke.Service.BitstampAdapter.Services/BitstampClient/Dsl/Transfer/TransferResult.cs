using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer
{
    public class TransferResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

    }
}
