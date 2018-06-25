using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    public sealed class OrderStatusResponse
    {
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("status")]
        public BitstampOrderStatus Status { get; set; }
        [JsonProperty("id")]
        public long OrderId { get; set; }
        [JsonProperty("transactions")]
        public IReadOnlyCollection<JObject> Transactions { get; set; }
    }
}
