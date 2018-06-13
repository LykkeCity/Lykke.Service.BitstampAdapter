namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public interface IApiCredentials
    {
        string Key { get; }
        byte[] Secret { get; }
        string UserId { get; }
    }
}