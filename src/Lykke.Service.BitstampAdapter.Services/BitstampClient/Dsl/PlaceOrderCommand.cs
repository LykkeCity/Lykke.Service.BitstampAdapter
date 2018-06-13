namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    public sealed class PlaceOrderCommand
    {
        public string Asset { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}
