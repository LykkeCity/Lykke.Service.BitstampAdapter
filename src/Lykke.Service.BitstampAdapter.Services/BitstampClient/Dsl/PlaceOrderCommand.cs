namespace Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl
{
    public sealed class PlaceOrderCommand
    {
        private string _asset;

        public string Asset
        {
            get => _asset;
            set => _asset = value?.ToLowerInvariant();
        }

        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}
