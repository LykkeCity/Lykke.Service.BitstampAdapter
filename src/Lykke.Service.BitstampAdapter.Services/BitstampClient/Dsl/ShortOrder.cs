using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    public sealed class BitstampDateTimeConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Func<string, object> fromString;
            if (objectType == typeof(DateTime))
            {
                fromString = x => DateTime.ParseExact(x, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (objectType == typeof(DateTimeOffset))
            {
                fromString = x => DateTimeOffset.ParseExact(x, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                throw new NotSupportedException();
            }

            var readAsString = (string)reader.Value;
            return fromString(readAsString);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset);
        }
    }

    public sealed class ShortOrder
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("currency_pair")]
        public string CurrencyPair { get; set; }

        [JsonProperty("datetime"), JsonConverter(typeof(BitstampDateTimeConverter))]
        public DateTime Datetime { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("type")]
        public BitstampOrderType Type { get; set; }
    }
}
