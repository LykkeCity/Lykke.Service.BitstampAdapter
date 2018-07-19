using System.Text;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public sealed class ApiCredentials : IApiCredentials
    {
        public ApiCredentials()
        {
        }

        public ApiCredentials(string key, string secret, string userId)
        {
            Key = key;
            UserId = userId;
            Secret = secret;
        }

        public string InternalApiKey { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string UserId { get; set; }
        public static IApiCredentials Empty => new ApiCredentials();

        byte[] IApiCredentials.Secret => Encoding.UTF8.GetBytes(Secret);
    }
}
