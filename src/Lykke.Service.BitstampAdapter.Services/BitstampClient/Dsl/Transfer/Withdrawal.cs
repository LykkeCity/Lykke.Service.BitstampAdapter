using System;
using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer
{
    public class Withdrawal
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("datetime")]
        public DateTime Datetime { get; set; }

        [JsonProperty("type")]
        public WithdrawalType Type { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
        
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("status")]
        public WithdrawalStatus Status {get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        public enum WithdrawalType
        {
            Sepa = 0,
            Bitcoin = 1,
            WireTransfer = 2,
            Xrp = 14,
            Litecoin = 15,
            Ethereum = 16
        }

        public enum WithdrawalStatus
        {
            Open = 0,
            InProcess = 1,
            Finished = 2,
            Canceled = 3,
            Failed = 4
        }

    }

}
