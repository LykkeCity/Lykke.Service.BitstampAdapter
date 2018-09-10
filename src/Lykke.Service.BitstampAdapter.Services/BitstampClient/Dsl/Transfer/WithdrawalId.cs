using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer
{
    public class WithdrawalId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
