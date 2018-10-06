using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.BitstampAdapter.Services;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Settings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "BitstampLykkeService API"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Swagger = swagger => swagger.ConfigureSwagger();
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "BitstampServiceLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.BitstampAdapterService.Db.LogsConnString;
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<BitstampAdapterSettings>();
            var httpClientFactory = app.ApplicationServices.GetService<HttpClientFactory>();
            XApiKeyAuthAttribute.Credentials = settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object) x);

            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x =>
                {
                    x.UseAuthenticationMiddleware(token =>
                        new ApiClient(GetCredentials(settings, token), httpClientFactory, token));
                    x.UseHandleBusinessExceptionsMiddleware();
                    x.UseForwardBitstampExceptionsMiddleware();
                };
            });

#if DEBUG
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif
        }

        private static ApiCredentials GetCredentials(BitstampAdapterSettings settings, string token)
        {
            return settings.Clients.FirstOrDefault(
                x => string.Equals(token, x.InternalApiKey, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
