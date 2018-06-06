using System;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.BitstampAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.BitstampAdapter
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {                                   
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.ApiTitle = "BitstampAdapter API";
                options.Logs = options.Logs = ("BitstampAdapterLog", ctx => ctx.BitstampAdapterService.Db.LogsConnString);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration();
        }
    }
}
