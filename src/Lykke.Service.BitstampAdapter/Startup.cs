using System;
using System.Linq;
using System.Security.Principal;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Sdk;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    public sealed class Startup : LykkeStartup<AppSettings>
    {
        protected override void ConfigureImpl(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<BitstampAdapterSettings>();
            XApiKeyAuthAttribute.Credentials = settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object) x);

            var log = app.ApplicationServices.GetService<ILog>();

            app.UseAuthenticationMiddleware(token => new ApiClient(GetCredentials(settings, token), log, token));
            app.UseHandleBusinessExceptionsMiddleware();
        }

        protected override void BuildServiceProvilder(LykkeServiceOptions<AppSettings> options)
        {
            options.ApiTitle = "BitstampAdapter API";
            options.Logs = ("BitstampAdapterLog", ctx => ctx.BitstampAdapterService.Db.LogsConnString);
        }

        private static ApiCredentials GetCredentials(BitstampAdapterSettings settings, string token)
        {
            return settings.Clients.FirstOrDefault(
                x => string.Equals(token, x.InternalApiKey, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
