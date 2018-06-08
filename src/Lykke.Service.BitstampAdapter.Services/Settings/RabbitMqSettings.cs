namespace Lykke.Service.BitstampAdapter.Services.Settings
{
    public sealed class RabbitMqSettings
    {
        public PublishSettings OrderBooks { get; set; }
        public PublishSettings TickPrices { get; set; }
    }
}
