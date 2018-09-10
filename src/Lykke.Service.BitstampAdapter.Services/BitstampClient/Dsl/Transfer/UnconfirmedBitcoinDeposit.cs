using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer
{
    public class UnconfirmedBitcoinDeposit
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }
    }
}
