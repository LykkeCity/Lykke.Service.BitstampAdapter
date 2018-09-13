using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BitstampAdapter.Extensions
{
    public static class ControllerExtensions
    {
        private const string CredentialsKey = "api-credentials";

        public static T GetRestApi<T>(this Controller controller)
        {
            if (controller.HttpContext.Items.TryGetValue(CredentialsKey, out object credentials))
                return (T) credentials;

            return default;
        }
    }
}
